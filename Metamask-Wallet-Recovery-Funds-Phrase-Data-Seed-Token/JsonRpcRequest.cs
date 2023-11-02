using System.Text.Json.Serialization;

namespace metamask_mini_api.Shared;

public record JsonRpcRequest(
    object id,
    string jsonrpc,
    string method,
    [property: JsonPropertyName("params")] object[]? parameters
);



