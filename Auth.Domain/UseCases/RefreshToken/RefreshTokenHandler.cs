using Auth.Domain.Entities;
using Auth.Domain.Exceptions;
using Auth.Domain.Services.Security;
using Auth.Domain.Services.Token;
using Auth.Domain.Shared;
using Auth.Domain.Storage;
using MediatR;

namespace Auth.Domain.UseCases.RefreshToken
{
    public class RefreshTokenHandler(
        IGuidProvider guidProvider,
        ICurrentUserProvider currentUserProvider,
        IRefreshTokenStorage refreshTokenStorage,
        IJwtProvider jwtProvider,
        IAuthenticateUserStorage authenticateUserStorage
        ) : IRequestHandler<RefreshTokenRequest, AuthenticateUserModel>
    {
        public async Task<AuthenticateUserModel> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
        {
            var userId = currentUserProvider.UserId;
            var tokenResult = await refreshTokenStorage.FindRefreshTokenAsync(userId, request.RefreshToken, cancellationToken);
            tokenResult.Handle(handleError: AppExceptions.Domain, handleNull: AppExceptions.Unauthorized);
            var findUserResult = await authenticateUserStorage.GetUserByIdAsync(userId, cancellationToken);
            findUserResult.Handle(handleError: AppExceptions.Domain, handleNull: () => AppExceptions.BadRequest("User not found"));

            var generateJwtResult = jwtProvider.GenerateJwtToken(findUserResult.Value);
            generateJwtResult.Handle(handleError: AppExceptions.Domain);

            var refreshToken = await GenerateTokenWithouCollisionAsync(request, findUserResult.Value, cancellationToken);

            var result = await refreshTokenStorage.RevokeRefreshTokenAsync(tokenResult.Value, refreshToken, "Issue new jwt token", request.Ip, cancellationToken);

            result.Handle(handleError: AppExceptions.Domain);

            return new AuthenticateUserModel(generateJwtResult.Value, refreshToken.Token);
        }

        private async ValueTask<Entities.RefreshToken> GenerateTokenWithouCollisionAsync(RefreshTokenRequest request, User user, CancellationToken cancellationToken)
        {
            Entities.RefreshToken refreshToken = GenerateRefreshToken(guidProvider.New(), user.Id, request.Ip);
            while (await refreshTokenStorage.ExistsAsync(user.Id, refreshToken.Token, cancellationToken))
            {
                refreshToken = GenerateRefreshToken(guidProvider.New(), user.Id, request.Ip);
            }

            return refreshToken;
        }

        private Entities.RefreshToken GenerateRefreshToken(Guid id, Guid userId, string ip)
        {
            var refreshTokenResult = jwtProvider.GenerateRefreshToken(id, userId, ip);
            refreshTokenResult.Handle(handleError: AppExceptions.Domain);
            return refreshTokenResult.Value;
        }
    }
}
