using ChatAPI.Core;

namespace ChatAPI.Repositories
{
    public interface IMessageRepository
    {
        Task<Message> AddMessageAsync(Message message);

        Task<IEnumerable<Message>> GetMessagesAsync();
    }
}
