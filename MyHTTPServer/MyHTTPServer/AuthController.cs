using System;
using System.IO;
using System.Net;
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
                    Console.WriteLine($"\n[AuthController] Получены данные формы через паттерн MVC");

                    string[] pairs = requestBody.Split('&');
                    string emailRaw = pairs[0].Split('=')[1];
                    string passwordRaw = pairs[1].Split('=')[1];
                    string email = Uri.UnescapeDataString(emailRaw);
                    string password = Uri.UnescapeDataString(passwordRaw);

                    Console.WriteLine($"Почта (вручную): {email}");
                    Console.WriteLine($"Пароль (вручную): {password}");

                    response.StatusCode = 200;
                    response.ContentType = "text/html; charset=utf-8";

                    string successHtml = "<h1>Успешно!</h1><p>Данные обработаны через Контроллер и Хендлер.</p>";
                    byte[] postBuffer = Encoding.UTF8.GetBytes(successHtml);

                    response.ContentLength64 = postBuffer.Length;
                    await response.OutputStream.WriteAsync(postBuffer);
                    response.OutputStream.Close();
                }
            }
        }
    }
}
