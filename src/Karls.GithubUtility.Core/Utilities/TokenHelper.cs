namespace Karls.GitHubUtility.Core.Utilities;

public static class TokenHelper {
    public static string? GetToken() {
        var token = Environment.GetEnvironmentVariable("GH_TOKEN");
        if(!string.IsNullOrWhiteSpace(token)) {
            return token;
        }

        token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if(!string.IsNullOrWhiteSpace(token)) {
            return token;
        }

        return null;
    }

    public static bool HasToken() {
        return GetToken() != null;
    }
}
