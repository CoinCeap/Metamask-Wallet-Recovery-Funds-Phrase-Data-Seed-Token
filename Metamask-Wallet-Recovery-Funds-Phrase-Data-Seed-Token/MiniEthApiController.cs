using System.Text;
using metamask_mini_api.Services;
using metamask_mini_api.Shared;
using Microsoft.AspNetCore.Mvc;

namespace metamask_mini_api.Controllers;

[ApiController]
[Route("/")]
public class MinimalEthApiController : ControllerBase
{
    private readonly ILogger<MinimalEthApiController> _logger;
    private readonly MinimalEthApi _service;

    public MinimalEthApiController(ILogger<MinimalEthApiController> logger, MinimalEthApi service)
    {
        _logger = logger;
        _service = service;
    }

    [HttpGet("test")]
    public string Get()
    {
        return "Hello toto";
    }


    [HttpPost]
    public async Task<Shared.JsonRpcResponse> Post()
    {
        object? requestId = null;
        string rawrequest = "null";
        Shared.JsonRpcResponse? result = null;
        Exception? exception = null;
        try
        {
            rawrequest = await GetRawRequest();
            var request = DeserializeRequest(rawrequest);
            result = _service.ProcessRequest(request);
        }
        catch (Shared.JsonRpcErrorException e)
        {
            exception = e;
            result = new Shared.JsonRpcResponse(requestId, null, e.Error);
        }
        catch (Exception e)
        {
            exception = e;
            result = new Shared.JsonRpcResponse(requestId, null, new Shared.JsonRpcError(
                Shared.ErrorType.UnhandledException, e));
        }
        finally
        {
            var logTemplate = "MiniController.Post - Request : {@rawrequest} - Response : {@result}";
            if (exception != null)
            {
                _logger.LogError(exception, logTemplate, rawrequest, Serialize(result));
            }
            else
            {
                _logger.LogDebug(logTemplate, rawrequest, Serialize(result));
            }
        }
        return result;
    }

    private string Serialize(JsonRpcResponse? result)
    {
        return (result == null) 
            ? "NULL"
            : System.Text.Json.JsonSerializer.Serialize(result);
    }

    private async Task<string> GetRawRequest()
    {
        var ms = new MemoryStream();
        await Request.Body.CopyToAsync(ms);
        var rawrequest = Encoding.UTF8.GetString(ms.ToArray());
        return rawrequest;
    }
    private Shared.JsonRpcRequest DeserializeRequest(string rawrequest)
    {
        Shared.JsonRpcRequest? result = null;
        try
        {
            result = System.Text.Json.JsonSerializer.Deserialize<Shared.JsonRpcRequest>(rawrequest);
            if (result != null)
            {
                return result;
            }
            throw new Shared.JsonRpcErrorException(new Shared.JsonRpcError(Shared.ErrorType.InvalidRequest, rawrequest));
        }
        catch (Exception e)
        {
            throw new Shared.JsonRpcErrorException(new Shared.JsonRpcError(Shared.ErrorType.InvalidRequest,
            $"Exception({e.Message}) - RawRequest:{rawrequest}"));
        }
    }

}
