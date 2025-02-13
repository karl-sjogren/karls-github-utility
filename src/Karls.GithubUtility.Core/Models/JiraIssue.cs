namespace Karls.GithubUtility.Core.Models;

public class JiraIssue {
    public required long Id { get; init; }
    public required string Key { get; init; }
    public required string Title { get; set; }
    public required JiraProject Project { get; set; }
    public required string Description { get; set; }
    public string[]? Labels { get; set; }
    public string? SprintName { get; set; }
    public string? Priority { get; set; }
    public string? Type { get; set; }
    public JiraUser? Assignee { get; set; }
    public required JiraUser Reporter { get; set; }
    public required DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
    public JiraComment[]? Comments { get; set; }
}
