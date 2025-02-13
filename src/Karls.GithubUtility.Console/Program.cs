using System.IO.Abstractions;
using Karls.GithubUtility.Console.Commands;
using Karls.GithubUtility.Console.Commands.JiraIssueMigration;
using Karls.GithubUtility.Console.Infrastructure;
using Karls.GithubUtility.Core.Contracts;
using Karls.GithubUtility.Core.Services;
using Karls.GitHubUtility.Core.Octokit;
using Karls.GitHubUtility.Core.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Octokit.Internal;
using Spectre.Console;
using Spectre.Console.Cli;

AnsiConsole.Clear();
AnsiConsole.Write(new FigletText("Karls GitHub Utility").Color(Color.Green));

ICredentialStore credentialStore;
if(TokenHelper.HasToken()) {
    credentialStore = new EnvironmentCredentialStore();
} else {
    AnsiConsole.MarkupLine("[red]No token found in environment variables.[/]");

    var token = AnsiConsole.Ask<string>("Enter your access token").Trim();
    if(string.IsNullOrWhiteSpace(token)) {
        AnsiConsole.MarkupLine("[bold red]No token entered, exiting.[/]");
        return 1;
    }

    credentialStore = new InMemoryCredentialStore(new Credentials(token));
    AnsiConsole.MarkupLine("[green]To skip entering the access token next time, please set the GH_TOKEN or GITHUB_TOKEN environment variable.[/]");
}

var client = new GitHubClient(new ProductHeaderValue("karls-githubutility"), credentialStore);

var services = new ServiceCollection();
services.AddLogging();
services.AddSingleton<IGitHubClient>(client);
services.AddSingleton<IFileSystem, FileSystem>();
services.AddSingleton(TimeProvider.System);

services.AddSingleton<IJiraExportParser, JiraExportParser>();

var registrar = new TypeRegistrar(services);

var app = new CommandApp(registrar);
app.Configure(config => {
    config.AddCommand<RemoveExpiredArtifactsCommand>("remove-expired-artifacts")
        .WithDescription("Remove expired artifacts from the specified repository.")
        .WithExample("remove-expired-artifacts -r owner/repo");

    config
        .AddBranch("jira-issue-migration", generate => {
            generate.SetDescription("Migrate issues from JIRA to Github.");

            generate.AddCommand<AnalyzeExportCommand>("analyze-export")
                .WithDescription("Analyze a JIRA export file.");

            generate.AddCommand<CreateUserMappingCommand>("user-mapping")
                .WithDescription("Setup mapping for JIRA users to Github users.");
        });
});

return app.Run(args);
