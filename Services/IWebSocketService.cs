using System.Net.WebSockets;

namespace ChatAPI.Services
{
    public interface IWebSocketService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task HandleWebSocketRequest(HttpContext context, WebSocket webSocket);
    }
}
