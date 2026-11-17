using System.Net;
using MiniHttpServer.Framework.Content;

namespace MiniHttpServer.Framework.Handlers;

public sealed class StaticFilesHandler
{
    private readonly string _root;
    public StaticFilesHandler(string root) => _root = Path.GetFullPath(root);

    public async Task<bool> TryHandleAsync(HttpListenerContext context,
        string relativePath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || relativePath == "/")
            relativePath = "/Steam/Steam.html";
        else if (!Path.HasExtension(relativePath))
            return false;

        var candidate = Path.GetFullPath(Path.Combine(_root,
            relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar)));

        if (!candidate.StartsWith(_root + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase) && candidate != _root)
            return false;
        if (!File.Exists(candidate)) return false;

        var bytes = await File.ReadAllBytesAsync(candidate, cancellationToken);
        context.Response.StatusCode = 200;
        context.Response.ContentType = MimeTypeProvider.Get(Path.GetExtension(candidate));
        context.Response.ContentLength64 = bytes.LongLength;
        await context.Response.OutputStream.WriteAsync(bytes, cancellationToken);
        return true;
    }
}
