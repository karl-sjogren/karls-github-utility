namespace Karls.GithubUtility.Core.Models;

public record JiraComment(long Id, JiraUser Author, DateTimeOffset CreatedAt, string Content);
