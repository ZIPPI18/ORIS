using MyHTTPServer;
using MyHTTPServer.Models;
using System.Text.Json;

Console.WriteLine("Инициализация приложения...");

string settingsJson = File.ReadAllText("settings.json");
Settings setting = JsonSerializer.Deserialize<Settings>(settingsJson);

HttpServer myServer = new HttpServer();
myServer.Start(setting);

Console.WriteLine("Введите 'stop' для завершения работы сервера...");

while (true)
{
    string command = Console.ReadLine();

    var orm = new OrmContext(setting.ConnectionString);

    int deleted = await orm.DeleteAsync("Users", 8);

    Console.WriteLine($"Удалено записей: {deleted}");

    if (command == "stop")
    {
        myServer.Stop();
        break;
    }
    else
    {
        Console.WriteLine($"Неизвестная команда '{command}'. Наберите 'stop' для выхода.");
    }
}

Console.WriteLine("Программа завершена.");