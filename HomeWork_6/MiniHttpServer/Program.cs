using MiniHttpServer.Framework;
using MiniHttpServer.Framework.Configuration;

var settings = ConfigurationManager.Load("settings.json");
var server = new HttpServer(settings);

Console.WriteLine("Введите stop для остановки сервера.");
server.Start();

while (true)
{
    var command = Console.ReadLine();
    if (string.Equals(command, "stop", StringComparison.OrdinalIgnoreCase))
        break;
}

server.Stop();
