using System.IO.Abstractions;
using System.Xml.Linq;
using Karls.GithubUtility.Core.Contracts;
using Karls.GithubUtility.Core.Extensions;
using Karls.GithubUtility.Core.Models;
using Microsoft.Extensions.Logging;

namespace Karls.GithubUtility.Core.Services;

public class JiraExportParser : IJiraExportParser {
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<JiraExportParser> _logger;

    public JiraExportParser(IFileSystem fileSystem, ILogger<JiraExportParser> logger) {
        _fileSystem = fileSystem;
        _logger = logger;
    }

    public async Task<JiraIssueExport> ParseAsync(string[] filenames, CancellationToken cancellationToken) {
        var export = new JiraIssueExport();

        foreach(var file in filenames) {
            await using var stream = _fileSystem.File.OpenRead(file);
            var doc = await XDocument.LoadAsync(stream, LoadOptions.None, cancellationToken);

            var root = doc.Root;
            if(root is null) {
                throw new InvalidOperationException("Root element is missing");
            }

            export.AddFile(file);

            var issues = root.Elements("channel").Elements("item");

            foreach(var issueNode in issues) {
                var title = issueNode.Element("summary")?.ValueOrDefault<string>(); // Use summary since it has the title without the key
                var key = issueNode.Element("key")?.ValueOrDefault<string>();
                var id = issueNode.Element("key")?.Attribute("id")?.ValueOrDefault<long>();

                if(title is null || key is null || id is null) {
                    _logger.LogWarning("Skipping issue with missing title, key or id.");
                    continue;
                }

                var reporterId = issueNode.Element("reporter")?.Attribute("accountid")?.ValueOrDefault<string>();
                var reporterName = issueNode.Element("reporter")?.ValueOrDefault<string>();

                if(reporterId is null || reporterName is null) {
                    _logger.LogWarning("Skipping issue with missing reporter information.");
                    continue;
                }

                var reporter = export.CreateOrUpdateUser(reporterId, reporterName);

                var assigneeId = issueNode.Element("assignee")?.Attribute("accountid")?.ValueOrDefault<string>();
                var assigneeName = issueNode.Element("assignee")?.ValueOrDefault<string>();

                var assignee = assigneeId is not null
                    ? export.CreateOrUpdateUser(assigneeId, assigneeName ?? string.Empty)
                    : null;

                var projectNode = issueNode.Element("project");

                var issue = new JiraIssue {
                    Id = id.GetValueOrDefault(),
                    Key = key,
                    Title = title,
                    Reporter = reporter,
                    CreatedAt = issueNode.Element("created")?.ValueOrDefault<DateTimeOffset>() ?? DateTimeOffset.MinValue,
                    UpdatedAt = issueNode.Element("updated")?.ValueOrDefault<DateTimeOffset>(),
                    Description = issueNode.Element("description")?.ValueOrDefault<string>() ?? string.Empty,
                    Project = export.CreateOrUpdateProject(
                        projectNode?.Attribute("id")?.ValueOrDefault<long>() ?? 0,
                        projectNode?.Attribute("key")?.ValueOrDefault<string>() ?? string.Empty,
                        projectNode?.ValueOrDefault<string>() ?? string.Empty
                    ),
                    SprintName = issueNode.Element("sprint")?.ValueOrDefault<string>(),
                    Priority = issueNode.Element("priority")?.ValueOrDefault<string>(),
                    Type = issueNode.Element("type")?.ValueOrDefault<string>(),
                    Labels = issueNode.Elements("label").Select(x => x.ValueOrDefault<string>()).OfType<string>().ToArray(),
                    Assignee = assignee,
                    Comments = issueNode.Elements("comment").Select(x => new JiraComment(
                        x.Attribute("id")?.ValueOrDefault<long>() ?? 0,
                        export.CreateOrUpdateUser(x.Attribute("accountid")?.ValueOrDefault<string>() ?? string.Empty, x.Attribute("author")?.ValueOrDefault<string>() ?? string.Empty),
                        x.Element("created")?.ValueOrDefault<DateTimeOffset>() ?? DateTimeOffset.MinValue,
                        x.ValueOrDefault<string>() ?? string.Empty
                    )).ToArray()
                };

                export.AddIssue(issue);
            }
        }

        return export;
    }
}

public class JiraIssueExportMapping {
    private readonly Dictionary<string, string> _userMapping = [];
    private readonly Dictionary<string, string> _labelMapping = [];

    public IReadOnlyDictionary<string, string> UserMapping => _userMapping.AsReadOnly();
    public IReadOnlyDictionary<string, string> LabelMapping => _labelMapping.AsReadOnly();

    public void AddUserMapping(string accountId, string githubUsername) {
        _userMapping.Add(accountId, githubUsername);
    }

    public void AddLabelMapping(string jiraLabel, string githubLabel) {
        _labelMapping.Add(jiraLabel, githubLabel);
    }
}
