using Karls.GitHubUtility.Core.Utilities;
using Octokit;

namespace Karls.GitHubUtility.Core.Octokit;

#pragma warning disable RCS1046 // Asynchronous method name should end with 'Async'.
public class EnvironmentCredentialStore : ICredentialStore {
    public Task<Credentials> GetCredentials() {
        var token = TokenHelper.GetToken();
        if(!string.IsNullOrWhiteSpace(token)) {
            return Task.FromResult(new Credentials(token));
        }

        throw new InvalidOperationException("No token found.");
    }
}
#pragma warning restore RCS1046
