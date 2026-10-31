using System;
using System.IO;
using System.Text.Json;
using MyHTTPServer;

string json = File.ReadAllText("settings.json");
Settings setting = JsonSerializer.Deserialize<Settings>(json);

HttpServer myServer = new HttpServer();
myServer.Start(setting);
Console.WriteLine("Сервер начал работу");

Console.WriteLine("Введите 'stop' для завершения работы сервера...");

while (true)
{
    string cmd = Console.ReadLine();
    if (cmd == "stop")
    {
        myServer.Stop();
        break;
    }
}
