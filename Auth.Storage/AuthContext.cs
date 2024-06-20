using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Auth.Storage
{
    public class AuthContext : DbContext
    {
        public AuthContext(DbContextOptions<AuthContext> opts)
            : base(opts)
        {
            
        }

        public AuthContext()
        {
        }

        public DbSet<Models.User> Users { get; set; }

        public DbSet<Models.RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}
