using System.Net;
using System.Threading.Tasks;

namespace MyHTTPServer
{
    public interface IController
    {
        Task HandleAsync(HttpListenerContext context, string cleanPath);
    }
}

