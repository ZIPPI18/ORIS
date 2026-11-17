using MiniHttpServer.Framework;
using MiniHttpServer.Framework.Configuration;

try
{
    var settings = ConfigurationManager.Load("settings.json");
    var server = new HttpServer(settings);
    server.Start();

    Console.WriteLine("Введите stop для остановки сервера.");
    while (true)
    {
        var command = Console.ReadLine();
        if (command == null || string.Equals(command, "stop", StringComparison.OrdinalIgnoreCase))
            break;
    }
    server.Stop();
}
catch (Exception ex)
{
    Console.Error.WriteLine("Ошибка запуска приложения: " + ex.Message);
}
