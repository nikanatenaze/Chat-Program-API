using ChatAppAPI.Data;
using ChatAppAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatAppAPI.Repository
{
    public class ChatUserRepository : ChatApiRepository<ChatUser>, IChatUserRepository
    {
        public ChatUserRepository(DataContext dbContext) : base(dbContext)
        {

        }
        public async Task<bool> IsUserInChat(int userId, int chatId)
        {
            return await _dbSet.AnyAsync(x => x.UserId == userId && x.ChatId == chatId);
        }

    }
}
