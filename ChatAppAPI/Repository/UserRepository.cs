using ChatAppAPI.Data;
using ChatAppAPI.Models;

namespace ChatAppAPI.Repository
{
    public class UserRepository : ChatApiRepository<User>, IUserRepository
    {
        public UserRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }
}
