using ChatAppAPI.Data;
using ChatAppAPI.Models;

namespace ChatAppAPI.Repository
{
    public class ChatRepository : ChatApiRepository<Chat>, IChatRepository
    {
        public ChatRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }

}
