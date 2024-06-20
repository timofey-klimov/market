using Auth.Domain.Entities;
using Auth.Domain.Exceptions;
using Auth.Domain.Services.Security;
using Auth.Domain.Services.Token;
using Auth.Domain.Shared;
using Auth.Domain.Storage;
using Auth.Domain.UseCases.RefreshToken;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Tests
{
    public class RefreshTokenShould
    {
        private RefreshTokenHandler sut;

        private Mock<IGuidProvider> guidProviderMock;
        private Mock<ICurrentUserProvider> currentUserProviderMock;
        private Mock<IRefreshTokenStorage> refreshTokenStorageMock;
        private Mock<IJwtProvider> jwtProviderMock;
        private Mock<IAuthenticateUserStorage> authenticateUserStorageMock;

        public RefreshTokenShould()
        {
            guidProviderMock = new Mock<IGuidProvider>();
            currentUserProviderMock = new Mock<ICurrentUserProvider>();
            refreshTokenStorageMock = new Mock<IRefreshTokenStorage>();
            jwtProviderMock = new Mock<IJwtProvider>();
            authenticateUserStorageMock = new Mock<IAuthenticateUserStorage>();
        }

        [Fact]
        public async Task ThrowUnauthorizedException_WhenTokenNotFound()
        {
            var request = new RefreshTokenRequest("Token", "Ip");
            refreshTokenStorageMock.Setup(x => x.FindRefreshTokenAsync(It.IsAny<Guid>(), request.RefreshToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(NullResult<RefreshToken>.Create());

            sut = new RefreshTokenHandler(
                guidProviderMock.Object, 
                currentUserProviderMock.Object, 
                refreshTokenStorageMock.Object, 
                jwtProviderMock.Object, 
                authenticateUserStorageMock.Object);

            await Assert.ThrowsAsync<UnauthorizedException>(() => sut.Handle(request, CancellationToken.None));
        }

        [Fact]
        public async Task ThrowBadRequest_WhenUserNotFound()
        {
            var request = new RefreshTokenRequest("Token", "Ip");
            refreshTokenStorageMock.Setup(x => x.FindRefreshTokenAsync(It.IsAny<Guid>(), request.RefreshToken, It.IsAny<CancellationToken>()))
                .ReturnsAsync(RefreshToken.Create(
                    new Guid("fa982886-7d62-4376-bfb9-4069b867f250"),
                    new Guid("45100838-2e68-4e98-9738-bbe0d5e12a4e"),
                    "Token",
                    DateTime.UtcNow.AddDays(1),
                    "Ip"));
            authenticateUserStorageMock.Setup(x => x.GetUserByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(NullResult<User>.Create());

            sut = new RefreshTokenHandler(
                guidProviderMock.Object,
                currentUserProviderMock.Object,
                refreshTokenStorageMock.Object,
                jwtProviderMock.Object,
                authenticateUserStorageMock.Object);


            await Assert.ThrowsAsync<BadRequestException>(() => sut.Handle(request, CancellationToken.None));
        }
    }
}
