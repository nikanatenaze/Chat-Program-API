using ChatAppAPI.Data;
using ChatAppAPI.Models;

namespace ChatAppAPI.Repository
{
    public class MessageRepository : ChatApiRepository<Message>, IMessageRepository
    {
        public MessageRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }
}
