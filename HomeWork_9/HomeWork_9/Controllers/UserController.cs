using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using MyHTTPServer.Models;

namespace MiniHttpServer.Controllers;

public sealed class UserController : IController
{
    private readonly string _connectionString;
    public UserController(string connectionString) => _connectionString = connectionString;

    public async Task HandleAsync(HttpListenerContext context, string cleanPath)
    {
        if (!context.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            await WriteAsync(context.Response, 405, "<h1>405 Метод не поддерживается</h1>");
            return;
        }

        try
        {
            var orm = new OrmContext(_connectionString);
            IReadOnlyList<User> users = await orm.GetAllAsync<User>("Users");
            string templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", "User", "List.html");
            string template = await File.ReadAllTextAsync(templatePath);
            var rows = new StringBuilder();
            foreach (var user in users)
                rows.Append("<li>").Append(HtmlEncoder.Default.Encode(user.Name))
                    .Append(" — ").Append(HtmlEncoder.Default.Encode(user.Email))
                    .Append(", возраст: ").Append(user.Age).Append("</li>");
            await WriteAsync(context.Response, 200,
                template.Replace("{{Users}}", rows.ToString(), StringComparison.Ordinal));
        }
        catch (Exception ex)
        {
            Console.WriteLine("[UserController] " + ex);
            await WriteAsync(context.Response, 500, "<h1>500 Ошибка при получении пользователей</h1>");
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
