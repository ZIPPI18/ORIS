using System.Net;
using MiniHttpServer.Framework.Results;

namespace MiniHttpServer.Framework.Controllers;

public abstract class BaseController
{
    protected HttpListenerContext Context { get; private set; } = null!;

    internal void Initialize(HttpListenerContext context) => Context = context;

    protected IHttpResult Html(string content, int statusCode = 200) =>
        new HtmlResult(content, statusCode);

    protected IHttpResult Json(object? data, int statusCode = 200) =>
        new JsonResult(data, statusCode);
}
