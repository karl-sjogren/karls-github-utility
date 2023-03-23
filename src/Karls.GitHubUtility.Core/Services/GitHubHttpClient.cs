using System.Net.Http.Headers;
using System.Net.Http.Json;
using Karls.GitHubUtility.Core.DataObjects;
using Octokit;

namespace Karls.GitHubUtility.Core.Services;

public class GitHubHttpClient {
    private readonly HttpClient _httpClient;

    public GitHubHttpClient(HttpClient httpClient, ICredentialStore credentialStore) {
        _httpClient = httpClient;

        var token = credentialStore.GetCredentials().Result.Password;
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", token);
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "GithubActionsArtifactHelper/1.0");
        _httpClient.BaseAddress = new Uri("https://api.github.com");
    }

    public async Task<ArtifactsDTO?> GetArtifactsAsync(string owner, string repository, Int32 page, Int32 pageSize = 100) {
        var url = $"/repos/{owner}/{repository}/actions/artifacts?per_page={pageSize}&page={page}";
        var response = await _httpClient.GetFromJsonAsync<ArtifactsDTO>(url);

        return response;
    }
}
