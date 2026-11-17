using System.Net;

namespace MiniHttpServer.Controllers;

public interface IController
{
    Task HandleAsync(HttpListenerContext context, string cleanPath);
}
