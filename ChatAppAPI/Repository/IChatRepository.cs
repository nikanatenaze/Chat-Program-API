using ChatAppAPI.Models;

namespace ChatAppAPI.Repository
{
    public interface IChatRepository : IChatApiRepository<Chat>
    {
        Task<bool> IsUserCreator(int userId, int chatId);
    }
}
