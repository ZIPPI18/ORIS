using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyHTTPServer
{
    public class HttpServer
    {
        private HttpListener _server = new HttpListener();
        private bool _isRunning;
        private Settings _setting;

        public void Start(Settings setting)
        {
            if (_isRunning) return;

            _setting = setting;

            string uriPrefix = $"http://{setting.Host}:{setting.Port}/{setting.Path}/";
            _server.Prefixes.Add(uriPrefix);

            _server.Start();
            _isRunning = true;
            Console.WriteLine("Cервер запущен и слушает: " + uriPrefix);

            Task.Run(async () => await ListenAsync());
        }

        private async Task ListenAsync()
        {
            try
            {
                while (_isRunning)
                {
                    var context = await _server.GetContextAsync();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    string localPath = request.Url.LocalPath;

                    string prefix = $"/{_setting.Path}/".Replace("//", "/");
                    if (localPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        localPath = localPath.Substring(prefix.Length - 1);
                    }

                    if (string.IsNullOrEmpty(localPath) || localPath == "/")
                    {
                        localPath = "/html.html";
                    }

                    string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "static", "Steam", localPath.TrimStart('/'));

                    if (!File.Exists(filePath))
                    {
                        response.StatusCode = 404;
                        byte[] errorBuffer = Encoding.UTF8.GetBytes($"<h1>404 Файл не найден</h1><p>Путь: {filePath}</p>");
                        response.ContentType = "text/html; charset=utf-8";
                        response.ContentLength64 = errorBuffer.Length;
                        await response.OutputStream.WriteAsync(errorBuffer);
                        response.OutputStream.Close();
                        continue;
                    }

                    string extension = Path.GetExtension(filePath);
                    response.ContentType = GetMimeType(extension);

                    byte[] buffer = await File.ReadAllBytesAsync(filePath);
                    response.ContentLength64 = buffer.Length;

                    using Stream output = response.OutputStream;
                    await output.WriteAsync(buffer);
                    await output.FlushAsync();

                    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Запрос успешно обработан.");
                }
            }
            catch (Exception ex)
            {
                if (_isRunning) Console.WriteLine("Ошибка сервера: " + ex.Message);
            }
        }

        private string GetMimeType(string extension)
        {
            switch (extension.ToLower())
            {
                case ".html": case ".htm": return "text/html; charset=utf-8";
                case ".css": return "text/css; charset=utf-8";
                case ".js": return "text/javascript; charset=utf-8";
                case ".json": return "application/json; charset=utf-8";
                case ".png": return "image/png";
                case ".jpg": case ".jpeg": return "image/jpeg";
                case ".gif": return "image/gif";
                case ".ico": return "image/x-icon";
                case ".svg": return "image/svg+xml";
                default: 
                    return "application/octet-stream"; 
            }
        }

        public void Stop()
        {
            if (!_isRunning) return;

            _isRunning = false;
            _server.Stop();
            _server.Prefixes.Clear();
            Console.WriteLine("Сервер успешно остановлен.");
        }
    }
}
