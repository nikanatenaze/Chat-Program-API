using System.Linq.Expressions;

namespace ChatAppAPI.Repository
{
    public interface IChatApiRepository<T>
    {
        Task<T> AddAsync(T dbRecord);

        Task<List<T>> GetAllAsync();

        Task<bool> RemoveAsync(T dbRecord);

        Task<T> GetAsync(Expression<Func<T, bool>> arguments);

        Task<T> UpdateAsync(T dbRecord);
    }
}
