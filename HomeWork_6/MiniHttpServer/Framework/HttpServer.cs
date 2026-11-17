using System.Net;
using MiniHttpServer.Framework.Configuration;
using MiniHttpServer.Framework.Handlers;

namespace MiniHttpServer.Framework;

public sealed class HttpServer
{
    private readonly ServerSettings _settings;
    private readonly HttpListener _listener = new();
    private RequestHandler? _handler;
    private volatile bool _running;

    public HttpServer(ServerSettings settings) => _settings = settings;

    public void Start()
    {
        if (_running) return;
        var prefix = $"http://{_settings.Host}:{_settings.Port}/{_settings.Path.Trim('/')}/";
        _listener.Prefixes.Add(prefix);
        _handler = new RequestHandler(_settings.StaticRoot);
        _listener.Start();
        _running = true;
        Console.WriteLine($"Сервер запущен: {prefix}");
        _ = ListenAsync();
    }

    private async Task ListenAsync()
    {
        while (_running)
        {
            try
            {
                var context = await _listener.GetContextAsync();
                _ = Task.Run(() => _handler!.ProcessAsync(context));
            }
            catch (HttpListenerException) when (!_running) { break; }
            catch (ObjectDisposedException) when (!_running) { break; }
            catch (Exception ex)
            {
                if (_running) Console.WriteLine($"Ошибка сервера: {ex.GetType().Name}");
            }
        }
    }

    public void Stop()
    {
        if (!_running) return;
        _running = false;
        _listener.Stop();
        _listener.Close();
        Console.WriteLine("Сервер остановлен.");
    }
}
