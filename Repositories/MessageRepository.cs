using ChatAPI.Core;

namespace ChatAPI.Repositories
{
    public class MessageRepository : IMessageRepository
    {
        public Task<Message> AddMessageAsync(Message message)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Message>> GetMessagesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
