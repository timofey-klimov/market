using Auth.Storage;
using Auth.Storage.Models;
using FluentAssertions;

namespace Auth.Tests.Storage
{
    public class RefreshTokenStorageTestFixture : StorageTestFixture
    {
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();

            var user = new User(Guid.Parse("1b2fe124-409f-46dd-b839-105039f17fc6"), "test", "test", "test", UserType.PickupPoint);
            var user1 = new User(Guid.Parse("c788aa6b-35c5-492b-a952-e26b0b519ace"), "test1", "test", "test", UserType.Seller);

            var avalableToken1 = new RefreshToken(
                id: Guid.Parse("c788aa6b-35c5-492b-a952-e26b0b519ace"),
                userId: Guid.Parse("1b2fe124-409f-46dd-b839-105039f17fc6"),
                token: "token1",
                expiresAt: DateTime.UtcNow.AddDays(1),
                createByIp: "ip");
            var availableToken2 = new RefreshToken(
                id: Guid.Parse("ba9aa96a-f554-45ba-8131-f030fc2f28e2"),
                userId: Guid.Parse("1b2fe124-409f-46dd-b839-105039f17fc6"),
                token: "token2",
                expiresAt: DateTime.UtcNow.AddDays(1),
                createByIp: "ip");

            var expiredToken1 = new RefreshToken(
                id: Guid.Parse("f018d116-99d2-406e-b93a-e82d8efdea50"),
                userId: Guid.Parse("1b2fe124-409f-46dd-b839-105039f17fc6"),
                token: "token3",
                expiresAt: DateTime.UtcNow.AddDays(-1),
                createByIp: "ip");

            var expiredToken2 = new RefreshToken(
                id: Guid.Parse("04225f2c-2a6d-4c11-b65f-16d2e90347a6"),
                userId: Guid.Parse("1b2fe124-409f-46dd-b839-105039f17fc6"),
                token: "token4",
                expiresAt: DateTime.UtcNow.AddDays(-1),
                createByIp: "ip");

            var context = GetDbContext();
            
            context.Users.AddRange(user, user1);
            context.RefreshTokens.AddRange(avalableToken1, availableToken2, expiredToken1, expiredToken2);
            await context.SaveChangesAsync();
        }
    }
    public class RefreshTokenStorageShould(RefreshTokenStorageTestFixture fixture) : IClassFixture<RefreshTokenStorageTestFixture>
    {

        [Fact]
        public async Task ReturnTrue_WhenExistsAndBelongToUser()
        {
            var token = "token3";
            var storage = new RefreshTokenStorage(fixture.GetDbContext());
            var result = await storage.ExistsAsync(Guid.Parse("1b2fe124-409f-46dd-b839-105039f17fc6"), token, CancellationToken.None);

            Assert.True(result);
        }

        [Fact]
        public async Task ReturnFalse_WhenExistsAndDontBelongToUser()
        {
            var token = "token3";
            var storage = new RefreshTokenStorage(fixture.GetDbContext());
            var result = await storage.ExistsAsync(Guid.Parse("c788aa6b-35c5-492b-a952-e26b0b519ace"), token, CancellationToken.None);

            Assert.False(result);
        }

        [Fact]
        public async Task RemoveAllBadTokens()
        {
            var storage = new RefreshTokenStorage(fixture.GetDbContext());
            await storage.RemoveAllExistsTokensAsync(Guid.Parse("1b2fe124-409f-46dd-b839-105039f17fc6"), CancellationToken.None);
            var tokens = fixture.GetDbContext().RefreshTokens.ToArray();

            foreach (var token in tokens)
            {
                token.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
            }
        }
    }
}
