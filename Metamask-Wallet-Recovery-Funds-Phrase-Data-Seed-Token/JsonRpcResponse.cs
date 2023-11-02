namespace metamask_mini_api.Shared;
public record JsonRpcResponse(
    object? id,
    object? result,
    JsonRpcError? error,
    string JsonRpc = "2.0");

public class JsonRpcError
{
    public JsonRpcError(ErrorType type, Exception exception) :
        this(type, $"Exception({exception.Message})-StackTrace:{exception.StackTrace}")
    { }

    public JsonRpcError(ErrorType type, object? errorData)
    {
        code = (int)type;
        message = type.ToString();
        data = errorData;
    }
    public JsonRpcError(ErrorType type, string message, object? errorData)
    {
        code = (int)type;
        this.message = message;
        data = errorData;
    }
    public int code { get; set; }
    public string message { get; set; }
    public object? data { get; set; }
}

public enum ErrorType
{
    UnhandledException,
    InvalidRequest,
    FailedTransaction,
    NotImplemented
}