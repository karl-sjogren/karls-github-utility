using Karls.GithubUtility.Core.Models;

namespace Karls.GithubUtility.Core.Contracts;

public interface IJiraExportParser {
    Task<JiraIssueExport> ParseAsync(string[] filenames, CancellationToken cancellationToken);
}
