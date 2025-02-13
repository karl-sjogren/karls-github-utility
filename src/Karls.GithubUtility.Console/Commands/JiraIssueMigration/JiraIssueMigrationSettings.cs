using System.ComponentModel;
using Spectre.Console.Cli;

namespace Karls.GithubUtility.Console.Commands.JiraIssueMigration;

public class JiraIssueMigrationSettings : CommandSettings {
    [CommandArgument(0, "<files>")]
    [Description("The exported issues file to run the command for.")]
    public required string[] ExportedIssuesFiles { get; set; }
}
