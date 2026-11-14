using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MyHTTPServer
{
    public class HomeworkController : IController
    {
        public async Task HandleAsync(HttpListenerContext context, string cleanPath)
        {
            var request = context.Request;
            var response = context.Response;

            if (request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase) &&
                cleanPath.Equals("/home-work/send", StringComparison.OrdinalIgnoreCase))
            {
                string toEmail = request.QueryString["to"];

                if (string.IsNullOrEmpty(toEmail))
                {
                    await SendResponseAsync(response, 400, "<h1>Ошибка 400</h1><p>Параметр 'to' обязателен.</p>");
                    return;
                }

                bool emailSent = SendHomeworkEmail(toEmail);

                if (emailSent)
                {
                    await SendResponseAsync(response, 200, "<h1>Успех</h1><p>Домашнее задание успешно отправлено!</p>");
                }
                else
                {
                    await SendResponseAsync(response, 500, "<h1>Ошибка сервера</h1><p>Не удалось отправить письмо.</p>");
                }
            }
            else
            {
                await SendResponseAsync(response, 404, "<h1>404 Не найдено</h1>");
            }
        }

        private bool SendHomeworkEmail(string targetEmail)
        {
            try
            {
                string? fromAddress = Environment.GetEnvironmentVariable("SMTP_USER");
                string? appPassword = Environment.GetEnvironmentVariable("SMTP_APP_PASSWORD");
                if (string.IsNullOrWhiteSpace(fromAddress) ||
                    string.IsNullOrWhiteSpace(appPassword))
                    throw new InvalidOperationException("Не заданы SMTP_USER и SMTP_APP_PASSWORD.");

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromAddress);
                    mail.To.Add(targetEmail);
                    mail.Subject = "Студент: Домашняя работа";

                    mail.Body = "Номер группы: 11-541\nФИО: Галиев Айрат Алмазович";
                    mail.IsBodyHtml = false;

                    string archivePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "project.zip");

                    if (File.Exists(archivePath))
                    {
                        Attachment attachment = new Attachment(archivePath);
                        mail.Attachments.Add(attachment);
                    }
                    else
                    {
                        Console.WriteLine($"[Warning] Файл архива не найден по пути: {archivePath}. Письмо уйдет без вложения.");
                    }

                    using (SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 587))
                    {
                        smtp.Credentials = new NetworkCredential(fromAddress, appPassword);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
                Console.WriteLine($"[SMTP] Домашняя работа успешно отправлена на {targetEmail}.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SMTP] Ошибка отправки домашней работы: " + ex.Message);
                return false;
            }
        }

        private async Task SendResponseAsync(HttpListenerResponse response, int statusCode, string htmlMessage)
        {
            response.StatusCode = statusCode;
            response.ContentType = "text/html; charset=utf-8";

            byte[] buffer = Encoding.UTF8.GetBytes(htmlMessage);
            response.ContentLength64 = buffer.Length;

            await response.OutputStream.WriteAsync(buffer);
            response.OutputStream.Close();
        }
    }
}
