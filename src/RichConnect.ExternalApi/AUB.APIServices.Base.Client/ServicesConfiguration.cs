using System.Text.Json.Serialization;

namespace AUB.APIServices.Base.Client;

public class ServicesConfiguration
{
    [JsonPropertyName("integrated")] public string Integrated { get; set; } = null!;
    [JsonPropertyName("basic")] public string Basic { get; set; } = null!;
    [JsonPropertyName("publicKeys")] public string PublicKeys { get; set; } = null!;
    [JsonPropertyName("tokenLifeTime")] public int TokenLifeTime { get; set; }
    [JsonPropertyName("signatureKeyLifeTime")] public int SignatureKeyLifeTime { get; set; }
    [JsonPropertyName("endPoints")] public Dictionary<string, string>? EndPoints { get; set; }
    [JsonIgnore] public bool NtlmEnabled { get; set; }
    [JsonIgnore] public bool ValidateSsl { get; set; }
    [JsonIgnore] public string Username { get; set; } = string.Empty;
    [JsonIgnore] public string Password { get; set; } = string.Empty;
}
