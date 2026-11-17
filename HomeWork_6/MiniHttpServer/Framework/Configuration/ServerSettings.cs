namespace MiniHttpServer.Framework.Configuration;

public sealed class ServerSettings
{
    public string Host { get; set; } = "127.0.0.1";
    public int Port { get; set; } = 9999;
    public string Path { get; set; } = "connection";
    public string StaticRoot { get; set; } = "static";
}
