using Auth.Domain;
using Auth.Domain.Entities;
using Auth.Domain.Shared;
using Auth.Storage;
using Auth.Storage.Models;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Tests.Storage
{
    public class AuthenticateStorageTestFixture : StorageTestFixture
    {
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await using var context = GetDbContext();

            var user1 = new Auth.Storage.Models.User(
                id: Guid.Parse("8B41C23E-123E-4F4A-93F0-BEBF9916C8B3"),
                login: "TestCustomer",
                password: "password",
                salt: "salt",
                Auth.Storage.Models.UserType.Customer);

            var user2 = new Auth.Storage.Models.User(
                id: Guid.Parse("85895444-65F3-47D8-857D-88F289E83D56"),
                login: "TestSeller",
                password: "password",
                salt: "salt",
                Auth.Storage.Models.UserType.Customer);

            context.Users.AddRange(user1, user2);
            await context.SaveChangesAsync();
        }
    }

    public class AuthenticateStorageShould(AuthenticateStorageTestFixture fixture) : IClassFixture<AuthenticateStorageTestFixture>
    {
        [Fact]
        public async Task ReturnUser_WhenDatabaseContainsWithSameLogin()
        {
            var storage = new AuthenticateUserStorage(fixture.GetDbContext());

            var userResult = await storage.GetUserByLoginAsync("TestCustomer", CancellationToken.None);

            userResult.Should().BeOfType<Result<Auth.Domain.Entities.User>>();
            userResult.Value.Login.Should().Be("TestCustomer");
        }

        [Fact]
        public async Task ReturnNull_WheneDatabaseDontContainsUser()
        {
            var storage = new AuthenticateUserStorage(fixture.GetDbContext());

            var userResult = await storage.GetUserByLoginAsync("123", CancellationToken.None);
            userResult.Should().BeOfType<NullResult<Domain.Entities.User>>();
        }
    }
}
