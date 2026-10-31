using System;
using System.Net;
using System.Threading.Tasks;

namespace MyHTTPServer
{
    public class HttpServer
    {
        private HttpListener _server = new HttpListener();
        private bool _isRunning;
        private MainHandler _handler;

        public void Start(Settings setting)
        {
            _handler = new MainHandler(setting);
            var controller = new ContestController();

            _handler.RegisterController("/entries", controller);
            _handler.RegisterController("/entries/details", controller);
            _handler.RegisterController("/entries/create", controller);
            _handler.RegisterController("/entries/update", controller);

            _server.Prefixes.Add($"http://{setting.Host}:{setting.Port}/{setting.Path}/");
            _server.Start();
            _isRunning = true;

            Task.Run(async () =>
            {
                while (_isRunning)
                {
                    var ctx = await _server.GetContextAsync();
                    Task.Run(async () => await _handler.ProcessRequestAsync(ctx));
                }
            });
        }

        public void Stop()
        {
            _isRunning = false;
            _server.Stop();
        }
    }
}
