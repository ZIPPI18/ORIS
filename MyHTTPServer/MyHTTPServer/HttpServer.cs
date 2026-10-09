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
        private MainHandler _handler; // Наш хендлер-сортировщик

        public void Start(Settings setting)
        {
            if (_isRunning) return;

            _setting = setting;

            // Создаем хендлер и динамически регистрируем в него наш контроллер авторизации!
            _handler = new MainHandler(setting);
            _handler.RegisterController("/login", new AuthController());
            // Если создадите новый класс (например, СhatController), просто допишете ниже:
            // _handler.RegisterController("/chat", new ChatController());

            string uriPrefix = $"http://{setting.Host}:{setting.Port}/{setting.Path}/";
            _server.Prefixes.Add(uriPrefix);

            _server.Start();
            _isRunning = true;
            Console.WriteLine("Сервер запущен через Паттерн Хендлер/Контроллеры: " + uriPrefix);

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
