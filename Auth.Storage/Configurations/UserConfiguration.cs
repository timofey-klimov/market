using Auth.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Auth.Storage.Configurations
{
    internal class UserConfiguration : IEntityTypeConfiguration<Models.User>
    {
        public void Configure(EntityTypeBuilder<Models.User> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Login)
                .HasMaxLength(30);

            builder.HasIndex(x => x.Login)
                .IsUnique();
        }
    }
}
