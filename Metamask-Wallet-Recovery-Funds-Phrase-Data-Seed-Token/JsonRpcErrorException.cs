using System.Runtime.Serialization;

namespace metamask_mini_api.Shared;
public class JsonRpcErrorException : Exception
{
    public JsonRpcError Error { get; set; }
    public JsonRpcErrorException(JsonRpcError error) : base(GetExceptionMessage(error))
    {
        Error = error;
    }

    private static string GetExceptionMessage(JsonRpcError error)
    {
        return $"{error.message} - {error.data}";
    }
}