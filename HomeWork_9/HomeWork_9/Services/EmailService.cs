using System.Net;
using System.Net.Mail;

namespace MiniHttpServer.Services;

public sealed class EmailService : IEmailService
{
    public async Task<bool> SendTestMessageAsync(string recipient)
    {
        var host = Environment.GetEnvironmentVariable("SMTP_HOST");
        var portText = Environment.GetEnvironmentVariable("SMTP_PORT");
        var user = Environment.GetEnvironmentVariable("SMTP_USER");
        var password = Environment.GetEnvironmentVariable("SMTP_PASSWORD");
        var from = Environment.GetEnvironmentVariable("SMTP_FROM");

        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) ||
            string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(from))
            return false;

        try
        {
            using var mail = new MailMessage(from, recipient)
            {
                Subject = "Тестовое сообщение",
                Body = "Это тестовое сообщение, отправленное из учебного HTTP-фреймворка."
            };
            using var smtp = new SmtpClient(host,
                int.TryParse(portText, out var port) ? port : 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(user, password)
            };
            await smtp.SendMailAsync(mail);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка отправки тестового письма: {ex.GetType().Name}");
            return false;
        }
    }
}
