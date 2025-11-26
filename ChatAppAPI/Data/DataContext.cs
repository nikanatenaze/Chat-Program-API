using ChatAppAPI.Data.Cunfigurations;
using ChatAppAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace ChatAppAPI.Data
{
    public class DataContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfig());
        }
    }
}
