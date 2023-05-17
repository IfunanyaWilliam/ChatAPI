using ChatAPI.Core;
using ChatAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ChatAPI.Controllers
{
    public class MessageController
    {
        private IWebSocketChatService _webSocketChatService;
        public MessageController(IWebSocketChatService webSocketChatService)
        {
            _webSocketChatService = webSocketChatService;
        }


        [HttpPost]
        public IActionResult Post(Message message)
        {
            // Set a new thread ID for the message if none is specified
            if (message.ThreadId == Guid.Empty)
            {
                message.ThreadId = Guid.NewGuid();
            }

            // Send the message to the WebSocket handler
            var httpContext = ControllerContext.HttpContext;
            var webSocket = httpContext.WebSockets.IsWebSocketRequest ?
                httpContext.WebSockets.AcceptWebSocketAsync().Result :
                null;

            if (webSocket != null)
            {
                httpContext.Items["ThreadId"] = message.ThreadId;
                _webSocketChatService.HandleWebSocketRequest(httpContext, webSocket).Wait();
            }

            return Ok();
        }
    }
}