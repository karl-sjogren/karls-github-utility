using Karls.GithubUtility.Core.Contracts;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Karls.GithubUtility.Console.Commands.JiraIssueMigration;

public sealed class AnalyzeExportCommand : AsyncCommand<AnalyzeExportCommand.Settings> {
    private readonly IJiraExportParser _jiraExportParser;

    public sealed class Settings : JiraIssueMigrationSettings {
    }

    public AnalyzeExportCommand(IJiraExportParser jiraExportParser) {
        _jiraExportParser = jiraExportParser;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings) {
        var file = settings.ExportedIssuesFiles;

        var export = await _jiraExportParser.ParseAsync(file, CancellationToken.None);

        AnsiConsole.MarkupLine($"[bold]Parsed [red]{file.Length}[/] files.[/]");

        AnsiConsole.MarkupLine($"[bold]Parsed [red]{export.Issues.Count}[/] issues from the export.[/]");

        AnsiConsole.MarkupLine($"[bold]Found [red]{export.Users.Count}[/] unique users in the export file.[/]");
        AnsiConsole.MarkupLine($"[bold]Found [red]{export.Projects.Count}[/] unique projects in the export file.[/]");
        AnsiConsole.MarkupLine($"[bold]Found [red]{export.Issues.Count}[/] unique issues in the export file.[/]");

        AnsiConsole.MarkupLine(string.Empty);
        AnsiConsole.MarkupLine("[bold]Users[/]");

        foreach(var user in export.Users) {
            AnsiConsole.MarkupLine($"  [bold]User [green]{user.Key}[/][/] {user.Value.Name}");
        }

        AnsiConsole.MarkupLine(string.Empty);
        AnsiConsole.MarkupLine("[bold]Projects[/]");

        foreach(var project in export.Projects) {
            AnsiConsole.MarkupLine($"  [bold]Project [green]{project.Key}[/][/] {project.Value.Name}");
        }

        AnsiConsole.MarkupLine(string.Empty);
        AnsiConsole.MarkupLine("[bold]Issues[/]");

        foreach(var issue in export.Issues) {
            AnsiConsole.MarkupLine($"  [bold]Issue [green]{issue.Key}[/][/] {Markup.Escape(issue.Title)}");
        }

        AnsiConsole.MarkupLine(string.Empty);
        AnsiConsole.MarkupLine("[bold]Summary[/]");
        AnsiConsole.MarkupLine($"Total Issues Processed: [green]{export.Issues.Count}[/]");

        return 0;
    }
}
