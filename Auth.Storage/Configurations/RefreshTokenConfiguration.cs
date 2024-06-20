using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Storage.Configurations
{
    internal class RefreshTokenConfiguration : IEntityTypeConfiguration<Models.RefreshToken>
    {
        public void Configure(EntityTypeBuilder<Models.RefreshToken> builder)
        {
            builder.HasKey(x => x.Id);

            builder.ToTable("RefreshTokens");
        }
    }
}
