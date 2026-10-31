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
        private Settings _setting;
        private Dictionary<string, IController> _routes = new Dictionary<string, IController>(StringComparer.OrdinalIgnoreCase);

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
                string cleanPath = request.Url.LocalPath.Replace("/" + _setting.Path, "").TrimEnd('/');
                string method = request.HttpMethod.ToUpper();

                if (_routes.TryGetValue(cleanPath, out var controller))
                {
                    await controller.HandleAsync(context, cleanPath);
                    return;
                }

                if (string.IsNullOrEmpty(cleanPath) || cleanPath == "")
                {
                    cleanPath = "/index.html";
                }
                else if (!cleanPath.Contains("."))
                {
                    cleanPath += "/index.html";
                }

                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "static", cleanPath.TrimStart('/'));

                if (!File.Exists(filePath))
                {
                    response.StatusCode = 404;
                    response.Close();
                    return;
                }

                byte[] buffer = await File.ReadAllBytesAsync(filePath);
                response.StatusCode = 200;
                response.ContentType = cleanPath.EndsWith(".js") ? "text/javascript" : "text/html";
                await response.OutputStream.WriteAsync(buffer);
                response.OutputStream.Close();
            }
            catch
            {
                response.StatusCode = 500;
                response.Close();
            }
        }
    }
}
