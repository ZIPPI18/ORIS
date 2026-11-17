using System.Net;
using MiniHttpServer.Controllers;
using MiniHttpServer.Framework.Configuration;

namespace MiniHttpServer.Framework.Handlers;

public sealed class RequestHandler
{
    private readonly ControllersHandler _controllers = new();
    private readonly StaticFilesHandler _staticFiles;
    private readonly Dictionary<string, IController> _customRoutes = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _pathPrefix;

    public RequestHandler(ServerSettings settings)
    {
        _staticFiles = new StaticFilesHandler(settings.StaticRoot);
        _pathPrefix = "/" + settings.Path.Trim('/') + "/";
        _customRoutes["GET:/user/list"] = new UserController(settings.ConnectionString);
        _customRoutes["GET:/home-work/send"] = new HomeworkController();
        _customRoutes["POST:/login"] = new AuthController();
    }

    public async Task ProcessAsync(HttpListenerContext context,
        CancellationToken cancellationToken = default)
    {
        var response = context.Response;
        try
        {
            string path = context.Request.Url?.AbsolutePath ?? "/";
            if (path.StartsWith(_pathPrefix, StringComparison.OrdinalIgnoreCase))
                path = "/" + path[_pathPrefix.Length..];
            if (path.Length > 1) path = path.TrimEnd('/');

            string key = context.Request.HttpMethod.ToUpperInvariant() + ":" + path;
            if (_customRoutes.TryGetValue(key, out var controller))
            {
                await controller.HandleAsync(context, path);
                return;
            }

            if (await _controllers.TryHandleAsync(context, cancellationToken))
                return;

            if (path == "/" || path.Equals("/login", StringComparison.OrdinalIgnoreCase))
                path = "/Steam/Steam.html";

            if (await _staticFiles.TryHandleAsync(context, path, cancellationToken))
                return;

            await NotFoundHandler.ExecuteAsync(response, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.WriteLine($"Ошибка обработки запроса: {ex}");
            try
            {
                if (response.OutputStream.CanWrite)
                    await NotFoundHandler.ExecuteAsync(response, cancellationToken);
            }
            catch { }
        }
        finally
        {
            try { response.Close(); } catch { }
        }
    }
}
