using ChatAppAPI.Data;
using ChatAppAPI.Models;

namespace ChatAppAPI.Repository
{
    public class ChatUserRepository : ChatApiRepository<ChatUser>, IChatUserRepository
    {
        public ChatUserRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }
}
