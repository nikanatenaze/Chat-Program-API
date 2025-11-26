using ChatAppAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAppAPI.Data.Cunfigurations
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).UseIdentityColumn(1, 1);
            builder.Property(x => x.Name).IsRequired(true).HasMaxLength(16);
            builder.Property(x => x.Password).IsRequired(true).HasMaxLength(24);
            builder.Property(x => x.Email).HasMaxLength(24);

            builder.HasData(new List<User>
            {
                new User()
                {
                    Id = 1,
                    Name = "John Doe",
                    Password = "paroli123",
                    Email = "JohnDoe123@gmail.com"
                }
            });
        }
    }
}
