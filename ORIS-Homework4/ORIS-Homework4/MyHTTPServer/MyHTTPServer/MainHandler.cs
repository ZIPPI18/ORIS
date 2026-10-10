using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyHTTPServer
{
    public class MainHandler
    {
        private readonly Settings _setting;
        private readonly Dictionary<string, IController> _routes = new Dictionary<string, IController>(StringComparer.OrdinalIgnoreCase);

        public MainHandler(Settings setting)
        {
            _setting = setting;
        }

        public void RegisterController(string path, IController controller)
        {
            _routes[path] = controller;
        }

        public async Task ProcessRequestAsync(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                string localPath = request.Url.LocalPath;
                string prefix = $"/{_setting.Path}/".Replace("//", "/");

                if (localPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    localPath = localPath.Substring(prefix.Length - 1);
                }

                string cleanPath = localPath.TrimEnd('/');

                if (string.IsNullOrEmpty(cleanPath) || cleanPath == "")
                {
                    localPath = "/Steam/Steam.html";
                    cleanPath = "/login";
                }
                else if (cleanPath.Equals("/login", StringComparison.OrdinalIgnoreCase))
                {
                    localPath = "/Steam/Steam.html";
                }
                else if (!cleanPath.Contains("."))
                {
                    localPath = cleanPath + "/index.html";
                }

                if (request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase) && _routes.ContainsKey(cleanPath))
                {
                    await _routes[cleanPath].HandleAsync(context, cleanPath);
                    return;
                }


                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "static", localPath.TrimStart('/'));

                if (!File.Exists(filePath))
                {
                    response.StatusCode = 404;
                    byte[] errorBuffer = Encoding.UTF8.GetBytes($"<h1>404 Файл не найден</h1><p>Путь: {filePath}</p>");
                    response.ContentType = "text/html; charset=utf-8";
                    response.ContentLength64 = errorBuffer.Length;
                    await response.OutputStream.WriteAsync(errorBuffer);
                    response.OutputStream.Close();
                    return;
                }

                string extension = Path.GetExtension(filePath);
                response.ContentType = GetMimeType(extension);

                byte[] buffer = await File.ReadAllBytesAsync(filePath);
                response.ContentLength64 = buffer.Length;

                using Stream output = response.OutputStream;
                await output.WriteAsync(buffer);
                await output.FlushAsync();
                response.OutputStream.Close();

                Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Успешно обработан файл: {localPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Ошибка обработки внутри Хендлера: " + ex.Message);
                response.StatusCode = 500;
                response.OutputStream.Close();
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
                default: return "application/octet-stream";
            }
        }
    }
}
