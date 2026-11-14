namespace MyHTTPServer;

public class Settings
{
    public string Port { get; set; } = "8888";
    public string Host { get; set; } = "localhost";
    public string Path { get; set; } = "connection";
    public string PublicDirectoryPath { get; set; } = "static";
    public string ConnectionString { get; set; } = string.Empty;
}
