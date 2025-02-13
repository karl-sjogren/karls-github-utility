using Karls.GithubUtility.Core.Models;

namespace Karls.GithubUtility.Core.Contracts;

public interface IJiraExportMapper {
    Task MapAsync(JiraIssueExport export, CancellationToken cancellationToken);
}
