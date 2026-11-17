using System.Text.Json;

namespace MiniHttpServer.Framework.Configuration;

public static class ConfigurationManager
{
    public static ServerSettings Load(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Не найден файл настроек сервера.", path);

        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var root = document.RootElement;
        var server = root.TryGetProperty("Server", out var section) ? section : root;

        return new ServerSettings
        {
            Host = GetString(server, "Host", "127.0.0.1"),
            Port = GetInt(server, "Port", 9999),
            Path = GetString(server, "Path", "connection"),
            StaticRoot = GetString(server, "StaticRoot", "static"),
            ConnectionString = GetString(root, "ConnectionString", string.Empty)
        };
    }

    private static string GetString(JsonElement e, string name, string fallback) =>
        e.TryGetProperty(name, out var value) ? value.ToString() : fallback;

    private static int GetInt(JsonElement e, string name, int fallback) =>
        e.TryGetProperty(name, out var value) && value.TryGetInt32(out var result) ? result : fallback;
}
