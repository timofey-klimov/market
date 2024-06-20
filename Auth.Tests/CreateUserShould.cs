using Auth.Domain;
using Auth.Domain.Entities;
using Auth.Domain.Exceptions;
using Auth.Domain.Services.Security;
using Auth.Domain.Services.Token;
using Auth.Domain.Shared;
using Auth.Domain.Storage;
using Auth.Domain.Storage.Transaction;
using Auth.Domain.UseCases.CreateUser;
using FluentAssertions;
using Moq;
using System.Data;

namespace Auth.Tests
{
    public class CreateUserShould
    {
        private CreateUserHandler sut;
        private Mock<IGuidProvider> guidProviderMock;
        private Mock<IRefreshTokenStorage> refreshTokenStorageMock;
        private Mock<ISecurityManager> securityManagerMock;
        private Mock<IJwtProvider> jwtProviderMock;
        private Mock<IAuthenticateUserStorage> userStorageMock;
        public CreateUserShould()
        {
            guidProviderMock = new Mock<IGuidProvider>();
            refreshTokenStorageMock = new Mock<IRefreshTokenStorage>();
            securityManagerMock = new Mock<ISecurityManager>();
            jwtProviderMock = new Mock<IJwtProvider>();
            userStorageMock = new Mock<IAuthenticateUserStorage>();
        }


        [Fact]
        public async Task ThrowBadRequestException_WhenePassworIsNullOrEmpty()
        {
            var request = new CreateUserRequest("Login", string.Empty, 0, "Ip");

            sut = new CreateUserHandler(
                refreshTokenStorage: refreshTokenStorageMock.Object,
                securityManager: securityManagerMock.Object,
                guidProvider: guidProviderMock.Object,
                authenticateUserStorage: userStorageMock.Object,
                jwtProvider: jwtProviderMock.Object);

            await Assert.ThrowsAsync<BadRequestException>(() => sut.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task ThrowBadRequestException_WhenLoginIsNotAvailable()
        {
            var request = new CreateUserRequest("Login", "Password", 0, "Ip");

            userStorageMock.Setup(x => x.IsLoginAvailableAsync(request.Login, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            sut = new CreateUserHandler(
                refreshTokenStorage: refreshTokenStorageMock.Object,
                securityManager: securityManagerMock.Object,
                guidProvider: guidProviderMock.Object,
                authenticateUserStorage: userStorageMock.Object,
                jwtProvider: jwtProviderMock.Object);

            await Assert.ThrowsAsync<BadRequestException>(() => sut.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task ThrowBadRequestException_WhenCreateUserReturnsErrorResult()
        {
            var request = new CreateUserRequest(string.Empty, "Password", 0, "Ip");

            userStorageMock.Setup(x => x.IsLoginAvailableAsync(request.Login, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            sut = new CreateUserHandler(
                refreshTokenStorage: refreshTokenStorageMock.Object,
                securityManager: securityManagerMock.Object,
                guidProvider: guidProviderMock.Object,
                authenticateUserStorage: userStorageMock.Object,
                jwtProvider: jwtProviderMock.Object);

            await Assert.ThrowsAsync<DomainException>(() => sut.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task ThrowDomainException_WhenGenerateJwtTokenReturnError()
        {
            var request = new CreateUserRequest("Login", "Password", 0, "Ip");

            userStorageMock.Setup(x => x.IsLoginAvailableAsync(request.Login, It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

            jwtProviderMock.Setup(x => x.GenerateJwtToken(It.IsAny<User>()))
                .Returns(Result<string>.FromErrors("Some error"));

            sut = new CreateUserHandler(
                refreshTokenStorage: refreshTokenStorageMock.Object,
                securityManager: securityManagerMock.Object,
                guidProvider: guidProviderMock.Object,
                authenticateUserStorage: userStorageMock.Object,
                jwtProvider: jwtProviderMock.Object);

            await Assert.ThrowsAsync<DomainException>(() => sut.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task ThrowDomainException_WhenGenerateRefreshTokenReturnError()
        {
            var request = new CreateUserRequest("Login", "Password", 0, "Ip");

            userStorageMock.Setup(x => x.IsLoginAvailableAsync(request.Login, It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

            jwtProviderMock.Setup(x => x.GenerateRefreshToken(It.IsAny<Guid>(), It.IsAny<Guid>(), request.Ip))
                .Returns(Result<RefreshToken>.FromErrors("Some error"));

            sut = new CreateUserHandler(
                refreshTokenStorage: refreshTokenStorageMock.Object,
                securityManager: securityManagerMock.Object,
                guidProvider: guidProviderMock.Object,
                authenticateUserStorage: userStorageMock.Object,
                jwtProvider: jwtProviderMock.Object);

            await Assert.ThrowsAsync<DomainException>(() => sut.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task ReturnAuthenticatedUserModel()
        {
            var jwtToken = "token";
            var resreshtokenResult = RefreshToken.Create(
                Guid.Parse("8dcbe7b2-6a66-4165-a6de-9ad494156a38"), 
                Guid.Parse("d45da971-ac07-4183-9258-7be5b951fff6"), 
                "token", 
                DateTime.UtcNow.AddDays(1), 
                "Ip");
            var request = new CreateUserRequest("Login", "Password", 0, "Ip");
            userStorageMock.Setup(x => x.IsLoginAvailableAsync(request.Login, It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);
            jwtProviderMock.Setup(x => x.GenerateJwtToken(It.IsAny<User>()))
                .Returns(Result<string>.FromValue(jwtToken));
            jwtProviderMock.Setup(x => x.GenerateRefreshToken(It.IsAny<Guid>(), It.IsAny<Guid>(), request.Ip))
                .Returns(resreshtokenResult);
            securityManagerMock.Setup(x => x.Hash(request.Password))
                .Returns(("hash", "salt"));
            guidProviderMock.Setup(x => x.New())
                .Returns(Guid.Parse("8dcbe7b2-6a66-4165-a6de-9ad494156a38"));

            sut = new CreateUserHandler(
                refreshTokenStorage: refreshTokenStorageMock.Object,
                securityManager: securityManagerMock.Object,
                guidProvider: guidProviderMock.Object,
                authenticateUserStorage: userStorageMock.Object,
                jwtProvider: jwtProviderMock.Object);

            var result = await sut.Handle(request, CancellationToken.None);

            result.Should().NotBeNull();
            result.RefreshToken.Should().BeEquivalentTo(resreshtokenResult.Value.Token);
            result.Token.Should().BeEquivalentTo(jwtToken);
        }
    }
}
