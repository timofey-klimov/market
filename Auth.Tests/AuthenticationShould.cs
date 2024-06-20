using Auth.Domain;
using Auth.Domain.Entities;
using Auth.Domain.Exceptions;
using Auth.Domain.Services.Security;
using Auth.Domain.Services.Token;
using Auth.Domain.Shared;
using Auth.Domain.Storage;
using Auth.Domain.UseCases.AuthorizeUser;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Language.Flow;

namespace Auth.Tests
{
    public class AuthenticationShould
    {
        private AuthenticateUserHandler useCase;
        private Mock<IAuthenticateUserStorage> mockUserStorage;
        private Mock<IRefreshTokenStorage> refreshTokenStorage;
        private Mock<IGuidProvider> guidProvider;
        private Mock<IJwtProvider> jwtProvider;
        private Mock<ISecurityManager> securityManager;
        private ISetup<IAuthenticateUserStorage, Task<IResult<User>>> authenticateUserStorageSetup;

        public AuthenticationShould()
        {
            mockUserStorage = new Moq.Mock<IAuthenticateUserStorage>();
            refreshTokenStorage = new Mock<IRefreshTokenStorage>();
            guidProvider = new Mock<IGuidProvider>();
            jwtProvider = new Mock<IJwtProvider>();
            securityManager =new Mock<ISecurityManager>();
        }

        [Fact]
        public void ShouldThrow_IfUserIsNull()
        {
            User user = null;

            authenticateUserStorageSetup = mockUserStorage
                .Setup(x => x.GetUserByLoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));

            authenticateUserStorageSetup.ReturnsAsync(Result<User>.FromValue(user));

            useCase = new AuthenticateUserHandler(
                guidProvider.Object,
                refreshTokenStorage.Object,
                securityManager.Object,
                jwtProvider.Object,
                mockUserStorage.Object);

            useCase.Invoking(x => x.Handle(new AuthenticateUserRequest("test", "test", "test"), CancellationToken.None))
                .Should().ThrowAsync<UnauthorizedException>();

        }



        [Fact]
        public async Task ShouldReturnToken()
        {
            var password = "TestPassword";
            var securityManager = new SecurityManager();

            var (hash, salt) = securityManager.Hash("TestPassword");

            var userResult = User.Create(
                new Guid("88fb67db-da5f-4636-9260-019ecb4fe6d7"),
                "User",
                hash,
                salt,
                UserType.Customer);
            var tokenSettings = new TokenSettings
            {
                RefreshTokenTTL = 10_080,
                TokenTTL = 15,
                Secret = "0viJlfIrq8bu62Tp+EvPyB4/TiUvp7MnDQE34bSLxeeEc7gEyWEVA4H1GGOn6mvqeOAcavf9Ow6U0O+QaD8ZlA=="
            };

            authenticateUserStorageSetup = mockUserStorage
                .Setup(x => x.GetUserByLoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));

            authenticateUserStorageSetup.ReturnsAsync(userResult);

            guidProvider.Setup(x => x.New())
                .Returns(new Guid("70fab0ef-0935-47df-9b35-106b41dd9c0d"));

            refreshTokenStorage
                .Setup(x => x.RemoveAllExistsTokensAsync(userResult.Value.Id, It.IsAny<CancellationToken>()));

            refreshTokenStorage.Setup(x => x.ExistsAsync(userResult.Value.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            refreshTokenStorage.Setup(x => x.CreateRefreshTokenAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()));
            refreshTokenStorage.Setup(x => x.FindAnyValidRefreshTokenAsync(userResult.Value.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(RefreshToken.Create(
                    new Guid("70fab0ef-0935-47df-9b35-106b41dd9c0d"),
                    new Guid("88fb67db-da5f-4636-9260-019ecb4fe6d7"),
                    "Token",
                    DateTime.UtcNow.AddDays(2), "IP"));



            useCase = new AuthenticateUserHandler(
                guidProvider.Object,
                refreshTokenStorage.Object,
                new SecurityManager(),
                new JwtProvider(Options.Create(tokenSettings)),
                mockUserStorage.Object);

            var result = await useCase.Handle(new AuthenticateUserRequest("test", password, "test"), CancellationToken.None);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Should_Throw_IfTokenEmpty()
        {
            var password = "TestPassword";
            var securityManagerObj = new SecurityManager();
            var (hash, salt) = securityManagerObj.Hash(password);

            var userResult = User.Create(
                new Guid("88fb67db-da5f-4636-9260-019ecb4fe6d7"),
                "User",
                hash,
                salt,
                UserType.Customer);

            authenticateUserStorageSetup = mockUserStorage
                .Setup(x => x.GetUserByLoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));

            authenticateUserStorageSetup.ReturnsAsync(userResult);

            securityManager.Setup(x => x.Verify(userResult.Value.Password, password, userResult.Value.Salt))
                .Returns(true);

            jwtProvider.Setup(x => x.GenerateJwtToken(userResult.Value))
                .Returns(Result<string>.FromErrors("Empty"));

            useCase = new AuthenticateUserHandler(
                guidProvider.Object,
                refreshTokenStorage.Object,
                securityManager.Object,
                jwtProvider.Object,
                mockUserStorage.Object);

            await Assert.ThrowsAsync<DomainException>(() => useCase.Handle(new AuthenticateUserRequest("test", password, "test"), CancellationToken.None));
        }

        [Fact]
        public async Task Should_Throw_IfRefreshTokenIsEmpty()
        {
            var password = "TestPassword";
            var securityManagerObj = new SecurityManager();
            var (hash, salt) = securityManagerObj.Hash(password);

            var userResult = User.Create(
                new Guid("88fb67db-da5f-4636-9260-019ecb4fe6d7"),
                "User",
                hash,
                salt, 
                UserType.Customer);

            authenticateUserStorageSetup = mockUserStorage
                .Setup(x => x.GetUserByLoginAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()));

            authenticateUserStorageSetup.ReturnsAsync(userResult);

            securityManager.Setup(x => x.Verify(userResult.Value.Password, password, userResult.Value.Salt))
                .Returns(true);

            jwtProvider.Setup(x => x.GenerateJwtToken(userResult.Value))
                .Returns(Result<string>.Bind(() => "token"));

            jwtProvider.Setup(x => x.GenerateRefreshToken(It.IsAny<Guid>(), userResult.Value.Id, It.IsAny<string>()))
                .Returns(Result<RefreshToken>.FromErrors("Error"));


            useCase = new AuthenticateUserHandler(
                guidProvider.Object,
                refreshTokenStorage.Object,
                securityManager.Object,
                jwtProvider.Object,
                mockUserStorage.Object);

            await Assert.ThrowsAsync<DomainException>(() => useCase.Handle(new AuthenticateUserRequest("test", password, "test"), CancellationToken.None));
        }


    }
}
