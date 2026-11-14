using MyHTTPServer;
using MyHTTPServer;
using System;
using System.IO;
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
