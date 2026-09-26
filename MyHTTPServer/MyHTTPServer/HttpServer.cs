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

        public void Start(Settings setting)
        {
            if (_isRunning) return;

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
                    HttpListenerResponse response = context.Response;

                    string htmlFileText = File.ReadAllText("hello.html");
                    byte[] buffer = Encoding.UTF8.GetBytes(htmlFileText);

                    response.ContentLength64 = buffer.Length;
                    response.ContentType = "text/html; charset=utf-8";

                    using Stream output = response.OutputStream;
                    await output.WriteAsync(buffer);
                    await output.FlushAsync();

                    Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Запрос успешно обработан.");
                }
            }
            catch (Exception ex) { Console.WriteLine("Ошибка сервера: " + ex.Message); }
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

