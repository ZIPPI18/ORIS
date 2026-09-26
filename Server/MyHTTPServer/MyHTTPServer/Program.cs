using MyHTTPServer;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

Console.WriteLine("Инициализация сервера...");

string settingsJson = File.ReadAllText("settings.json");
Settings setting = JsonSerializer.Deserialize<Settings>(settingsJson);

string UriPrefix = $"http://{setting.Host}:{setting.Port}/{setting.Path}/";

HttpListener server = new HttpListener();
server.Prefixes.Add(UriPrefix);
server.Start();

Console.WriteLine("Cервер запущен и слушает: " + UriPrefix);
Console.WriteLine("Введите 'stop' в консоль для корректного выключения.\n");

bool isRunning = true;

_ = Task.Run(async () =>
{
    try
    {
        while (isRunning)
        {
            var context = await server.GetContextAsync();

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
    catch (Exception ex) { Console.WriteLine("Ошибка: " + ex.Message); }
});


while (true)
{
    string command = Console.ReadLine();

    if (command == "stop")
    {
        Console.WriteLine("Остановка сервера...");
        isRunning = false;
        server.Stop();
        break;
    }
    else
    {
        Console.WriteLine($"Неизвестная команда '{command}'. Наберите 'stop' для корректного выхода.");
    }
}

Console.WriteLine("Сервер завершил работу. Программа успешно закрыта.");

