using ChatAppAPI.Models;

namespace ChatAppAPI.Repository
{
    public interface IChatUserRepository : IChatApiRepository<ChatUser>
    {
        Task<bool> IsUserInChat(int userId, int chatId);
    }
}
