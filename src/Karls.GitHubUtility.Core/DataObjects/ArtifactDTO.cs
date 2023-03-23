using System.Text.Json.Serialization;

namespace Karls.GitHubUtility.Core.DataObjects;

public record ArtifactDTO(
    Int32 Id,
    string Name,
    bool Expired,
    [property: JsonPropertyName("size_in_bytes")] long Size,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt
    );
