using ChatAPI.Core;
using ChatAPI.Repositories;
using Newtonsoft.Json;
using System.Linq;

namespace ChatAPI.Services
{
    public class WebSocketChatService : IWebSocketChatService
    {
        private readonly WebSocketHandler _webSocketHandler;
        private readonly IMessageRepository _messageRepository;

        public WebSocketChatService(WebSocketHandler webSocketHandler, IMessageRepository messageRepository)
        {
            _webSocketHandler = webSocketHandler;
            _messageRepository = messageRepository;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var messages = await _messageRepository.GetMessagesAsync();
                var threads = messages.Select(m => m.ThreadId).Distinct();

                foreach (var thread in threads)
                {
                    var chatMessages = messages.Where(m => m.ThreadId == thread).ToList();
                    var chatMessagesJson = JsonConvert.SerializeObject(chatMessages);

                    await _webSocketHandler.BroadcastAsync(thread, chatMessagesJson);
                }

                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }
    }
}
