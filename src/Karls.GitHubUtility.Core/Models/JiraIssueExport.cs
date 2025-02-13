namespace Karls.GithubUtility.Core.Models;

public class JiraIssueExport {
    private readonly List<string> _files = [];
    private readonly List<JiraIssue> _issues = [];
    private readonly Dictionary<long, JiraProject> _projects = [];
    private readonly Dictionary<string, JiraUser> _users = [];

    public IReadOnlyCollection<string> Files => _files.AsReadOnly();
    public IReadOnlyCollection<JiraIssue> Issues => _issues.AsReadOnly();
    public IReadOnlyDictionary<long, JiraProject> Projects => _projects.AsReadOnly();
    public IReadOnlyDictionary<string, JiraUser> Users => _users.AsReadOnly();

    public JiraUser CreateOrUpdateUser(string accountId, string name) {
        Users.TryGetValue(accountId, out var user);
        if(user is null) {
            user = new JiraUser(accountId, name);
            _users.Add(accountId, user);
        } else if(!string.IsNullOrWhiteSpace(name) && user.Name != name) {
            _users[accountId] = user with { Name = name };
        }

        return user;
    }

    public JiraProject CreateOrUpdateProject(long id, string key, string name) {
        Projects.TryGetValue(id, out var project);
        if(project is null) {
            project = new JiraProject(id, key, name);
            _projects.Add(id, project);
        } else if(project.Key != key || !string.IsNullOrWhiteSpace(name) && project.Name != name) {
            _projects[project.Id] = project with { Key = key, Name = name };
        }

        return project;
    }

    public void AddFile(string file) {
        _files.Add(file);
    }

    public void AddIssue(JiraIssue issue) {
        _issues.Add(issue);
    }
}
