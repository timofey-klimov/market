using Auth.Storage;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace Auth.Tests.Storage
{
    public class StorageTestFixture : IAsyncLifetime
    {
        private readonly PostgreSqlContainer dbContainer = new PostgreSqlBuilder()
            .WithPortBinding("5432", false)
            .WithPassword("admin")
            .Build();
        public AuthContext GetDbContext() => new(new DbContextOptionsBuilder<AuthContext>()
            .UseNpgsql(dbContainer.GetConnectionString()).Options);

        public async Task DisposeAsync() => await dbContainer.DisposeAsync();

        public virtual async Task InitializeAsync()
        {
            await dbContainer.StartAsync();
            var forumDbContext = new AuthContext(new DbContextOptionsBuilder<AuthContext>()
                .UseNpgsql(dbContainer.GetConnectionString()).Options);
            await forumDbContext.Database.MigrateAsync();
        }
    }
}
