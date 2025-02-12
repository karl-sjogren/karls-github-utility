using Humanizer;
using Karls.GitHubUtility.Core.Octokit;
using Karls.GitHubUtility.Core.Utilities;
using Octokit;
using Octokit.Internal;
using Spectre.Console;

AnsiConsole.Clear();
AnsiConsole.Write(new FigletText("Karls GitHub Utility").Color(Color.Green));

var timeProvider = TimeProvider.System;

ICredentialStore credentialStore;
if(TokenHelper.HasToken()) {
    credentialStore = new EnvironmentCredentialStore();
} else {
    AnsiConsole.MarkupLine("[red]No token found in environment variables.[/]");

    var token = AnsiConsole.Ask<string>("Enter your access token").Trim();
    if(string.IsNullOrWhiteSpace(token)) {
        AnsiConsole.MarkupLine("[bold red]No token entered, exiting.[/]");
        return;
    }

    credentialStore = new InMemoryCredentialStore(new Credentials(token));
    AnsiConsole.MarkupLine("[green]To skip entering the access token next time, please set the GH_TOKEN or GITHUB_TOKEN environment variable.[/]");
}

var client = new GitHubClient(new ProductHeaderValue("karls-githubutility"), credentialStore);

var user = await client.User.Current();

var projectFullName = AnsiConsole.Ask<string>("Enter repository name (e.g. owner/repo)").Trim();
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
            response = await client.Actions.Artifacts.ListArtifacts(owner, repository, request);
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
    return;
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

        var artifactsToRemove = artifacts.Where(x => x.Name == artifactName && x.Expired && x.CreatedAt < timeProvider.GetUtcNow().AddDays(-1)).ToArray();
        task.MaxValue = artifactsToRemove.Length;

        foreach(var item in artifactsToRemove) {
            try {
                await client.Actions.Artifacts.DeleteArtifact(owner, repository, item.Id);
                task.Increment(1);
            } catch(Exception ex) {
                AnsiConsole.WriteException(ex);
                break;
            }
        }
    });
