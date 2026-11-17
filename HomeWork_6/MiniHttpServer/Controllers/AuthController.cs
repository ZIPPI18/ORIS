using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;
using MiniHttpServer.Services;

namespace MiniHttpServer.Controllers;

[Controller]
public sealed class AuthController : BaseController
{
    private readonly IEmailService _emailService = new EmailService();

    [Post]
    public async Task<IHttpResult> SendEmail(string? email, string? password)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            return Json(new { Success = false, Message = "Укажите корректный тестовый адрес электронной почты" }, 400);

        var sent = await _emailService.SendTestMessageAsync(email);
        return sent
            ? Json(new { Success = true, Message = "Сообщение отправлено" })
            : Json(new { Success = false, Message = "Не удалось отправить сообщение" }, 500);
    }
}
