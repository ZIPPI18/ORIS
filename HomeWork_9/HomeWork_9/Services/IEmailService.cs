namespace MiniHttpServer.Services;

public interface IEmailService
{
    Task<bool> SendTestMessageAsync(string recipient);
}
