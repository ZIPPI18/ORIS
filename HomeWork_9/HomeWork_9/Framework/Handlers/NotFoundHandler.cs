using System.Net;
using System.Text;

namespace MiniHttpServer.Framework.Handlers;

public static class NotFoundHandler
{
    public static async Task ExecuteAsync(HttpListenerResponse response,
        CancellationToken cancellationToken = default)
    {
        var bytes = Encoding.UTF8.GetBytes("<h1>404 Not Found</h1>");
        response.StatusCode = 404;
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes, cancellationToken);
    }
}
