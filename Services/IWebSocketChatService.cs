namespace ChatAPI.Services
{
    public interface IWebSocketChatService
    {
        Task StartAsync(CancellationToken cancellationToken);
    }
}
