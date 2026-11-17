using System.Net;

namespace MiniHttpServer.Framework.Handlers;

public sealed class RequestHandler
{
    private readonly ControllersHandler _controllers = new();
    private readonly StaticFilesHandler _staticFiles;

    public RequestHandler(string staticRoot) => _staticFiles = new StaticFilesHandler(staticRoot);

    public async Task ProcessAsync(HttpListenerContext context,
        CancellationToken cancellationToken = default)
    {
        var response = context.Response;
        try
        {
            var path = context.Request.Url?.AbsolutePath ?? "/";
            const string prefix = "/connection";
            if (path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                path = path[prefix.Length..];
            if (string.IsNullOrEmpty(path)) path = "/";

            if (await _controllers.TryHandleAsync(context, cancellationToken))
                return;

            if (await _staticFiles.TryHandleAsync(context, path, cancellationToken))
                return;

            await NotFoundHandler.ExecuteAsync(response, cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.WriteLine($"Ошибка обработки запроса: {ex.GetType().Name}");
            try
            {
                if (response.OutputStream.CanWrite)
                    await NotFoundHandler.ExecuteAsync(response, cancellationToken);
            }
            catch { /* Клиент мог уже закрыть соединение. */ }
        }
        finally
        {
            try { response.Close(); } catch { }
        }
    }
}
