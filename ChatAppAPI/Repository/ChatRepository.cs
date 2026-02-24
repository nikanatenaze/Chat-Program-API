using ChatAppAPI.Data;
using ChatAppAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatAppAPI.Repository
{
    public class ChatRepository : ChatApiRepository<Chat>, IChatRepository
    {
        public ChatRepository(DataContext dbContext) : base(dbContext)
        {
        }

        public async Task<bool> IsUserCreator(int userId, int chatId)
        {
            return await _dbContext.Chats.AnyAsync(x => x.Id == chatId && x.CreatedByUserId == userId);
        }
    }

}
