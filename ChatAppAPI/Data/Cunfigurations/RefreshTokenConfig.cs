using ChatAppAPI.Models.TokenModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAppAPI.Data.Cunfigurations
{
    public class RefreshTokenConfig : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).UseIdentityColumn(1, 1);

            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.ExpiryDate).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.Token).IsRequired();
            builder.Property(x => x.IsRevoked).IsRequired();
        }
    }
}
