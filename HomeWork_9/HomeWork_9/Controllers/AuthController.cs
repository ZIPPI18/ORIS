using System.Net;
using System.Text;

namespace MiniHttpServer.Controllers;

public sealed class AuthController : IController
{
    public async Task HandleAsync(HttpListenerContext context, string cleanPath)
    {
        if (!context.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            await WriteAsync(context.Response, 405, "<h1>405 Метод не поддерживается</h1>");
            return;
        }

        using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
        string body = await reader.ReadToEndAsync();
        var form = ParseForm(body);
        string email = form.GetValueOrDefault("email", "");
        string password = form.GetValueOrDefault("password", "");
        Console.WriteLine($"[AuthController] Получена форма авторизации для {email}; пароль не сохраняется.");
        string message = string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password)
            ? "<h1>Заполните email и пароль</h1>"
            : "<h1>Форма получена</h1><p>Данные приняты для демонстрации.</p>";
        await WriteAsync(context.Response, string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ? 400 : 200, message);
    }

    private static Dictionary<string,string> ParseForm(string body)
    {
        var result = new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
        foreach (string part in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = part.Split('=', 2);
            result[Uri.UnescapeDataString(pair[0].Replace("+", " "))] =
                pair.Length > 1 ? Uri.UnescapeDataString(pair[1].Replace("+", " ")) : "";
        }
        return result;
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
