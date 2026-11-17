using System.Collections;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.Json;
using MiniHttpServer.Framework.Attributes;
using MiniHttpServer.Framework.Controllers;
using MiniHttpServer.Framework.Results;

namespace MiniHttpServer.Framework.Handlers;

public sealed class ControllersHandler
{
    private sealed record ActionInfo(Type ControllerType, MethodInfo Method);
    private readonly Dictionary<string, ActionInfo> _routes =
        new(StringComparer.OrdinalIgnoreCase);

    public ControllersHandler()
    {
        var types = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract &&
                        t.GetCustomAttribute<ControllerAttribute>() != null &&
                        typeof(BaseController).IsAssignableFrom(t));

        foreach (var type in types)
        {
            var controllerName = type.GetCustomAttribute<ControllerAttribute>()!.Name
                ?? (type.Name.EndsWith("Controller", StringComparison.Ordinal)
                    ? type.Name[..^10] : type.Name);
            foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                         .Where(m => m.DeclaringType != typeof(object)))
            {
                var verb = method.GetCustomAttribute<GetAttribute>() != null ? "GET"
                    : method.GetCustomAttribute<PostAttribute>() != null ? "POST" : null;
                if (verb == null) continue;
                var path = $"/{controllerName}/{method.Name}".ToLowerInvariant();
                _routes[$"{verb}:{path}"] = new ActionInfo(type, method);
            }
        }

        if (_routes.TryGetValue("POST:/auth/sendemail", out var sendEmail))
            _routes["POST:/login"] = sendEmail;
    }

    public async Task<bool> TryHandleAsync(HttpListenerContext context,
        CancellationToken cancellationToken = default)
    {
        var request = context.Request;
        var path = request.Url?.AbsolutePath ?? "/";
        var key = $"{request.HttpMethod}:{path}".ToLowerInvariant();
        if (!_routes.TryGetValue(key, out var action)) return false;

        try
        {
            var controller = (BaseController?)Activator.CreateInstance(action.ControllerType);
            if (controller == null) throw new InvalidOperationException("Не удалось создать контроллер.");
            controller.Initialize(context);

            var arguments = await BindArgumentsAsync(action.Method, context, cancellationToken);
            var returned = action.Method.Invoke(controller, arguments);

            if (returned is Task task)
            {
                await task;
                returned = task.GetType().GetProperty("Result")?.GetValue(task);
            }

            if (returned is not IHttpResult result)
            {
                context.Response.StatusCode = 500;
                return true;
            }

            await result.ExecuteAsync(context.Response, cancellationToken);
        }
        catch (TargetInvocationException)
        {
            await WriteErrorAsync(context.Response, 500, "Внутренняя ошибка сервера");
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.WriteLine($"Ошибка контроллера: {ex.GetType().Name}");
            if (context.Response.OutputStream.CanWrite)
                await WriteErrorAsync(context.Response, 500, "Внутренняя ошибка сервера");
        }
        return true;
    }

    private static async Task<object?[]> BindArgumentsAsync(MethodInfo method,
        HttpListenerContext context, CancellationToken cancellationToken)
    {
        var parameters = method.GetParameters();
        var result = new object?[parameters.Length];
        string body = "";
        if (context.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            using var reader = new StreamReader(context.Request.InputStream,
                context.Request.ContentEncoding);
            body = await reader.ReadToEndAsync(cancellationToken);
        }

        Dictionary<string, string> form = ParseForm(body);
        JsonDocument? json = null;
        if (!string.IsNullOrWhiteSpace(body) &&
            context.Request.ContentType?.Contains("application/json",
                StringComparison.OrdinalIgnoreCase) == true)
        {
            try { json = JsonDocument.Parse(body); } catch { }
        }

        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];
            if (p.ParameterType == typeof(HttpListenerContext))
            {
                result[i] = context;
                continue;
            }

            string? raw = context.Request.QueryString[p.Name ?? ""] ??
                          (form.TryGetValue(p.Name ?? "", out var value) ? value : null);
            if (raw == null && json?.RootElement.ValueKind == JsonValueKind.Object &&
                json.RootElement.TryGetProperty(p.Name ?? "", out var property))
                raw = property.ToString();

            result[i] = ConvertValue(raw, p.ParameterType, p.HasDefaultValue ? p.DefaultValue : null);
        }
        json?.Dispose();
        return result;
    }

    private static Dictionary<string, string> ParseForm(string body)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = part.Split('=', 2);
            var key = Uri.UnescapeDataString(pair[0].Replace("+", " "));
            var value = pair.Length > 1
                ? Uri.UnescapeDataString(pair[1].Replace("+", " ")) : "";
            values[key] = value;
        }
        return values;
    }

    private static object? ConvertValue(string? value, Type type, object? fallback)
    {
        if (value == null) return fallback ?? (type.IsValueType ? Activator.CreateInstance(type) : null);
        if (type == typeof(string)) return value;
        return Convert.ChangeType(value, Nullable.GetUnderlyingType(type) ?? type,
            CultureInfo.InvariantCulture);
    }

    private static async Task WriteErrorAsync(HttpListenerResponse response, int status, string message)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(new { success = false, message }));
        response.StatusCode = status;
        response.ContentType = "application/json; charset=utf-8";
        response.ContentLength64 = bytes.Length;
        await response.OutputStream.WriteAsync(bytes);
    }
}
