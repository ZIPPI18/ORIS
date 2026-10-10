using System;
using System.Net;
using System.Threading.Tasks;

namespace MyHTTPServer
{
    public class HttpServer
    {
        private HttpListener _server = new HttpListener();
        private bool _isRunning;
        private Settings _setting;
        private MainHandler _handler;

        public void Start(Settings setting)
        {
            if (_isRunning) return;

            _setting = setting;

            _handler = new MainHandler(setting);
            _handler.RegisterController("/login", new AuthController());


            string uriPrefix = $"http://{setting.Host}:{setting.Port}/{setting.Path}/";
            _server.Prefixes.Add(uriPrefix);

            _server.Start();
            _isRunning = true;
            Console.WriteLine("Сервер запущен: " + uriPrefix);

            Task.Run(async () => await ListenAsync());
        }

        private async Task ListenAsync()
        {
            try
            {
                while (_isRunning)
                {
                    var context = await _server.GetContextAsync();

                    Task.Run(async () => await _handler.ProcessRequestAsync(context));
                }
            }
            catch (Exception ex)
            {
                if (_isRunning) Console.WriteLine("Ошибка сервера: " + ex.Message);
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
