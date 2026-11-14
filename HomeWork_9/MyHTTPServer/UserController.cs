using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.Data.SqlClient;
using MyHTTPServer.Models;

namespace MyHTTPServer;

public sealed class UserController : IController
{
    private readonly string _connectionString;

    public UserController(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        _connectionString = connectionString;
    }

    public async Task HandleAsync(HttpListenerContext context, string cleanPath)
    {
        if (!context.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase) ||
            !cleanPath.Equals("/user/list", StringComparison.OrdinalIgnoreCase))
        {
            await WriteResponseAsync(context.Response, 405,
                "<h1>405 Метод не поддерживается</h1>");
            return;
        }

        try
        {
            List<User> users = await LoadUsersAsync(_connectionString);

            string templatePath = Path.Combine(AppContext.BaseDirectory,
                "Templates", "User", "List.html");
            string template = await File.ReadAllTextAsync(templatePath);

            var rows = new StringBuilder();
            foreach (User user in users)
            {
                rows.Append("<li>")
                    .Append(HtmlEncoder.Default.Encode(user.Name))
                    .Append(" — ")
                    .Append(HtmlEncoder.Default.Encode(user.Email))
                    .Append(", возраст: ")
                    .Append(user.Age)
                    .Append("</li>");
            }

            string html = template.Replace("{{Users}}", rows.ToString(),
                StringComparison.Ordinal);
            await WriteResponseAsync(context.Response, 200, html);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[UserController] " + ex);
            await WriteResponseAsync(context.Response, 500,
                "<h1>500 Ошибка при получении пользователей</h1>");
        }
    }

    public static async Task<List<User>> LoadUsersAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        var users = new List<User>();

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        const string sql = "SELECT Id, Name, Email, Age FROM dbo.Users;";
        await using var command = new SqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        int idOrdinal = reader.GetOrdinal("Id");
        int nameOrdinal = reader.GetOrdinal("Name");
        int emailOrdinal = reader.GetOrdinal("Email");
        int ageOrdinal = reader.GetOrdinal("Age");

        while (await reader.ReadAsync(cancellationToken))
        {
            users.Add(new User
            {
                Id = reader.GetInt32(idOrdinal),
                Name = reader.GetString(nameOrdinal),
                Email = reader.GetString(emailOrdinal),
                Age = reader.GetInt32(ageOrdinal)
            });
        }

        return users;
    }

    private static async Task WriteResponseAsync(
        HttpListenerResponse response, int statusCode, string html)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(html);
        response.StatusCode = statusCode;
        response.ContentType = "text/html; charset=utf-8";
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes);
        response.Close();
    }
}
