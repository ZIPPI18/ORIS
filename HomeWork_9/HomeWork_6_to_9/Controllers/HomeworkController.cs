using System.Net;
using System.Net.Mail;
using System.Text;

namespace MiniHttpServer.Controllers;

public sealed class HomeworkController : IController
{
    public async Task HandleAsync(HttpListenerContext context, string cleanPath)
    {
        string? target = context.Request.QueryString["to"];
        if (string.IsNullOrWhiteSpace(target) || !target.Contains('@'))
        {
            await WriteAsync(context.Response, 400, "<h1>Ошибка 400</h1><p>Параметр 'to' обязателен и должен быть email.</p>");
            return;
        }

        try
        {
            string from = Environment.GetEnvironmentVariable("SMTP_USER") ?? "";
            string password = Environment.GetEnvironmentVariable("SMTP_APP_PASSWORD") ?? "";
            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(password))
                throw new InvalidOperationException("Задайте SMTP_USER и SMTP_APP_PASSWORD.");

            using var mail = new MailMessage(from, target)
            {
                Subject = "Студент: Домашняя работа",
                Body = "Номер группы: 11-541\nФИО: Галиев Айрат Алмазович",
                IsBodyHtml = false
            };
            string archive = Path.Combine(AppContext.BaseDirectory, "project.zip");
            if (File.Exists(archive)) mail.Attachments.Add(new Attachment(archive));

            using var smtp = new SmtpClient("smtp.yandex.ru", 587)
            {
                Credentials = new NetworkCredential(from, password),
                EnableSsl = true
            };
            await smtp.SendMailAsync(mail);
            await WriteAsync(context.Response, 200, "<h1>Успех</h1><p>Домашнее задание отправлено.</p>");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[HomeworkController] " + ex.Message);
            await WriteAsync(context.Response, 500, "<h1>Не удалось отправить письмо. Проверьте SMTP-настройки.</h1>");
        }
    }

    private static async Task WriteAsync(HttpListenerResponse response, int status, string html)
    {
        var bytes = Encoding.UTF8.GetBytes(html);
        response.StatusCode = status;
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes);
    }
}
