using ChatAppAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ChatAppAPI.Repository
{
    public class ChatApiRepository<T> : IChatApiRepository<T> where T : class
    {
        private readonly DataContext _dbContext;
        private DbSet<T> _dbSet;

        public ChatApiRepository(DataContext dbContext)
        {
            _dbContext = dbContext;
            _dbSet = dbContext.Set<T>();
        }

        public async Task<T> AddAsync(T dbRecord)
        {
            await _dbSet.AddAsync(dbRecord);
            await _dbContext.SaveChangesAsync();
            return dbRecord;
        }
        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<List<T>> GetAllAsync(Expression<Func<T, bool>> arguments)
        {
            return await _dbSet.Where(arguments).ToListAsync();
        }

        public async Task<bool> RemoveAsync(T dbRecord)
        {
            _dbSet.Remove(dbRecord);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> arguments)
        {
            return await _dbSet.AsNoTracking().FirstOrDefaultAsync(arguments);
        }

        public async Task<T> UpdateAsync(T dbRecord)
        {
            _dbContext.Update(dbRecord);
            await _dbContext.SaveChangesAsync();
            return dbRecord;
        }
    }
}
