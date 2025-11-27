using ChatAppAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAppAPI.Data.Cunfigurations
{
    public class ChatUserConfig : IEntityTypeConfiguration<ChatUser>
    {
        public void Configure(EntityTypeBuilder<ChatUser> builder)
        {
            builder.ToTable("ChatUsers");

            builder.HasKey(x => new { x.UserId, x.ChatId });

            builder.Property(x => x.JoinedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasOne(x => x.User)
                .WithMany(x => x.ChatUsers)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Chat)
                .WithMany(x => x.ChatUsers)
                .HasForeignKey(x => x.ChatId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
