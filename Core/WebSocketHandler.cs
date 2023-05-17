using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using ChatAPI.Repositories;

namespace ChatAPI.Core
{
    public class WebSocketHandler
    {
        private readonly ConcurrentDictionary<Guid, WebSocket> _sockets = new ConcurrentDictionary<Guid, WebSocket>();
        private readonly ConcurrentDictionary<Guid, Guid> _socketThreads = new ConcurrentDictionary<Guid, Guid>();
        private readonly IMessageRepository _messageRepository;

        public WebSocketHandler(IMessageRepository messageRepository)
        {
            _messageRepository = messageRepository;
        }

        public async Task HandleWebSocketRequest(HttpContext context, WebSocket webSocket)
        {
            Guid socketId = Guid.NewGuid();
            _sockets.TryAdd(socketId, webSocket);

            Guid threadId = GetThreadIdFromHttpContext(context);

            if (threadId == Guid.Empty)
            {
                threadId = Guid.NewGuid();
                SetThreadIdInHttpContext(context, threadId);
            }

            _socketThreads.TryAdd(socketId, threadId);

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var buffer = new ArraySegment<byte>(new byte[1024]);
                    var result = await webSocket.ReceiveAsync(buffer, context.RequestAborted);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer.Array, buffer.Offset, result.Count);
                        var chatMessage = JsonConvert.DeserializeObject<Message>(message);

                        // Store the message in the database using a new thread
                        await Task.Factory.StartNew(() => _messageRepository.AddMessageAsync(chatMessage));

                        // Broadcast the message to all connected clients in the same thread
                        await BroadcastAsync(threadId, message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "WebSocket closed", context.RequestAborted);
                        _sockets.TryRemove(socketId, out _);
                        _socketThreads.TryRemove(socketId, out _);
                    }
                }
            }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
            {
                // Ignore this exception and simply log it
                Console.WriteLine($"WebSocket connection closed prematurely: {ex.Message}");
            }
        }

        public async Task BroadcastAsync(Guid threadId, string message)
        {
            foreach (var socket in _sockets)
            {
                if (socket.Value.State == WebSocketState.Open && _socketThreads.TryGetValue(socket.Key, out Guid socketThreadId) && socketThreadId == threadId)
                {
                    var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
                    await socket.Value.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        private Guid GetThreadIdFromHttpContext(HttpContext context)
        {
            return context.Items.TryGetValue("ThreadId", out object threadIdObj) && threadIdObj is Guid threadId ? threadId : Guid.Empty;
        }

        private void SetThreadIdInHttpContext(HttpContext context, Guid threadId)
        {
            context.Items["ThreadId"] = threadId;
        }
    }
}