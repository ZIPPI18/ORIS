using System.Net;
using System.Text;
using System.Text.Json;

namespace MiniHttpServer.Framework.Results;

public sealed class JsonResult : IHttpResult
{
    private readonly object? _data;
    private readonly int _statusCode;
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public JsonResult(object? data, int statusCode = 200)
    {
        _data = data;
        _statusCode = statusCode;
    }

    public async Task ExecuteAsync(HttpListenerResponse response,
        CancellationToken cancellationToken = default)
    {
        if (response.OutputStream.CanWrite == false) return;

        var json = JsonSerializer.Serialize(_data, Options);
        var bytes = Encoding.UTF8.GetBytes(json);
        response.StatusCode = _statusCode;
        response.ContentType = "application/json; charset=utf-8";
        response.ContentLength64 = bytes.LongLength;
        await response.OutputStream.WriteAsync(bytes, cancellationToken);
    }
}
