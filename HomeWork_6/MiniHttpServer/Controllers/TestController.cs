using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;

namespace MiniHttpServer.Controllers;

[Controller]
public sealed class TestController : BaseController
{
    [Get]
    public IHttpResult Index() =>
        Html("<h1>Hello from controller!</h1>");

    [Get]
    public IHttpResult Info() =>
        Json(new { Id = 1, Message = "Hello World!", Success = true });

    [Get]
    public IHttpResult RequestInfo() =>
        Json(new
        {
            Method = Context.Request.HttpMethod,
            Path = Context.Request.Url?.AbsolutePath ?? "/",
            UserAgent = Context.Request.Headers["User-Agent"]
        });
}
