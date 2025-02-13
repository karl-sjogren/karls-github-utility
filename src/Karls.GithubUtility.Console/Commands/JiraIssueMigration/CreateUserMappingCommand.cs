using Octokit;
using Spectre.Console.Cli;

namespace Karls.GithubUtility.Console.Commands.JiraIssueMigration;

public sealed class CreateUserMappingCommand : AsyncCommand<CreateUserMappingCommand.Settings> {
    private readonly IGitHubClient _gitHubClient;
    private readonly TimeProvider _timeProvider;

    public sealed class Settings : JiraIssueMigrationSettings {
    }

    public CreateUserMappingCommand(IGitHubClient gitHubClient, TimeProvider timeProvider) {
        _gitHubClient = gitHubClient;
        _timeProvider = timeProvider;
    }

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings) {
        return Task.FromResult(0);
    }
}
