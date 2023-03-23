using System.Text.Json.Serialization;

namespace Karls.GitHubUtility.Core.DataObjects;

public record ArtifactsDTO(
    [property: JsonPropertyName("artifacts")] ArtifactDTO[] Items,
    [property: JsonPropertyName("total_count")] long TotalCount
    );
