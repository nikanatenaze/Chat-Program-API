using ChatAppAPI.Data;
using ChatAppAPI.Models;

namespace ChatAppAPI.Repository
{
    public class UserRepository : ChatApiRepository<User>, IUserReporitory
    {
        public UserRepository(DataContext dbContext) : base(dbContext)
        {
        }
    }
}
