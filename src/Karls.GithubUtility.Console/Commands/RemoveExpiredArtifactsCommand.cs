using System.ComponentModel;
using Humanizer;
using Octokit;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Karls.GithubUtility.Console.Commands;

public sealed class RemoveExpiredArtifactsCommand : AsyncCommand<RemoveExpiredArtifactsCommand.Settings> {
    private readonly IGitHubClient _gitHubClient;
    private readonly TimeProvider _timeProvider;

    public sealed class Settings : CommandSettings {
        [CommandOption("-r|--repository")]
        [Description("Repository name (e.g. owner/repo) to fetch artifacts from.")]
        public string? Repository { get; set; }
    }

    public RemoveExpiredArtifactsCommand(IGitHubClient gitHubClient, TimeProvider timeProvider) {
        _gitHubClient = gitHubClient;
        _timeProvider = timeProvider;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
        var projectFullName = settings.Repository ?? AnsiConsole.Ask<string>("Enter repository name (e.g. owner/repo)").Trim();
        if(string.IsNullOrWhiteSpace(projectFullName) || !projectFullName.Contains('/', StringComparison.Ordinal)) {
            AnsiConsole.MarkupLine("[red]Invalid repository name entered.[/]");
            return 1;
        }

        var owner = projectFullName.Split('/')[0];
        var repository = projectFullName.Split('/')[1];

        var artifacts = new List<Artifact>();

        await AnsiConsole
            .Progress()
            .AutoClear(true)
            .Columns([
                new TaskDescriptionColumn(),
        new ProgressBarColumn(),
        new PercentageColumn(),
        new RemainingTimeColumn(),
        new SpinnerColumn()
            ])
            .StartAsync(async ctx => {
                var task = ctx.AddTask($"[green]Fetching artifact metadata from {projectFullName}[/]");

                var page = 0;
                var pageSize = 100;

                ListArtifactsResponse? response;
                do {
                    var request = new ListArtifactsRequest { PerPage = pageSize, Page = ++page };
                    response = await _gitHubClient.Actions.Artifacts.ListArtifacts(owner, repository, request);
                    if(response is null || response.Artifacts is null || response.Artifacts.Count == 0) {
                        break;
                    }

                    artifacts.AddRange(response.Artifacts);

                    task.MaxValue = response.TotalCount;
                    task.Increment(response.Artifacts.Count);
                } while(response.Artifacts.Count >= pageSize);
            });

        var expirationBreakdown = artifacts.GroupBy(x => x.Expired).ToDictionary(x => x.Key, x => x.Sum(x => x.SizeInBytes));

        var breakdownChart = new BreakdownChart()
            .Width(110)
            .UseValueFormatter((x, _) => x.Bytes().Humanize("#.##"));

        foreach(var (expired, size) in expirationBreakdown) {
            breakdownChart.AddItem(expired ? "Expired" : "Active", size, expired ? Color.Red : Color.Green);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(breakdownChart);

        var grid = new Grid();

        grid.AddColumn();
        grid.AddColumn();
        grid.AddColumn();

        grid.AddRow(["[blue]Artifact name[/]", "[green]Active size[/]", "[red]Expired size[/]"]);

        var artifactNames = artifacts.Select(x => x.Name).Distinct().OrderBy(x => x).ToArray();

        foreach(var name in artifactNames) {
            var activeSize = artifacts.Where(x => x.Name == name && !x.Expired).Sum(x => x.SizeInBytes);
            var expiredSize = artifacts.Where(x => x.Name == name && x.Expired).Sum(x => x.SizeInBytes);

            grid.AddRow([name, activeSize.Bytes().Humanize("#.##"), expiredSize.Bytes().Humanize("#.##")]);
        }

        AnsiConsole.WriteLine();
        AnsiConsole.Write(grid);

        var prompt = new SelectionPrompt<string>()
                .Title("Which artifact do you [red]want to remove[/]?")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to see all options)[/]")
                .AddChoices(artifacts.Select(x => x.Name).Distinct().OrderBy(x => x).ToArray());
        AnsiConsole.WriteLine();
        var artifactName = AnsiConsole.Prompt(prompt);

        if(!AnsiConsole.Confirm("Are you sure?", false)) {
            return 0;
        }

        await AnsiConsole
            .Progress()
            .Columns([
                new TaskDescriptionColumn(),
        new ProgressBarColumn(),
        new PercentageColumn(),
        new RemainingTimeColumn(),
        new SpinnerColumn()
            ])
            .StartAsync(async ctx => {
                var task = ctx.AddTask($"[red]Removing artifacts with name {artifactName}[/]");

                var artifactsToRemove = artifacts.Where(x => x.Name == artifactName && x.Expired && x.CreatedAt < _timeProvider.GetUtcNow().AddDays(-1)).ToArray();
                task.MaxValue = artifactsToRemove.Length;

                foreach(var item in artifactsToRemove) {
                    try {
                        await _gitHubClient.Actions.Artifacts.DeleteArtifact(owner, repository, item.Id);
                        task.Increment(1);
                    } catch(Exception ex) {
                        AnsiConsole.WriteException(ex);
                        break;
                    }
                }
            });

        return 0;
    }
}
