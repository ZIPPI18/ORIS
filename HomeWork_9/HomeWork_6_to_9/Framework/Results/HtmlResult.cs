using System.Net;
using System.Text;

namespace MiniHttpServer.Framework.Results;

public sealed class HtmlResult : IHttpResult
{
    private readonly string _content;
    private readonly int _statusCode;

    public HtmlResult(string content, int statusCode = 200)
    {
        _content = content ?? string.Empty;
        _statusCode = statusCode;
    }

    public async Task ExecuteAsync(HttpListenerResponse response,
        CancellationToken cancellationToken = default)
    {
        if (response.OutputStream.CanWrite == false) return;

        var bytes = Encoding.UTF8.GetBytes(_content);
        response.StatusCode = _statusCode;
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = bytes.LongLength;
        await response.OutputStream.WriteAsync(bytes, cancellationToken);
    }
}
