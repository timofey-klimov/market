using Auth.Domain.Entities;
using Auth.Domain.Exceptions;
using Auth.Domain.Services.Security;
using Auth.Domain.Services.Token;
using Auth.Domain.Shared;
using Auth.Domain.Storage;
using MediatR;

namespace Auth.Domain.UseCases.AuthorizeUser
{
    public class AuthenticateUserHandler(
        IGuidProvider guidProvider,
        IRefreshTokenStorage refreshTokenStorage,
        ISecurityManager securityManager,
        IJwtProvider jwtProvider,
        IAuthenticateUserStorage userStorage) : IRequestHandler<AuthenticateUserRequest, AuthenticateUserModel>
    {
        public async Task<AuthenticateUserModel> Handle(AuthenticateUserRequest request, CancellationToken cancellationToken)
        {
            var userResult = await userStorage.GetUserByLoginAsync(request.Login, cancellationToken);
            userResult.Handle(
                handleError: AppExceptions.Domain,
                handleNull: () => AppExceptions.BadRequest("Invalid user credentials"));

            var user = userResult.Value;
            if (!securityManager.Verify(user.Password, request.Password, user.Salt))
                throw new UnauthorizedException();

            var jwtTokenResult = jwtProvider.GenerateJwtToken(user);
            jwtTokenResult.Handle(handleError: AppExceptions.Domain);

            var findTokenResult = await refreshTokenStorage.FindAnyValidRefreshTokenAsync(user.Id, cancellationToken);

            Domain.Entities.RefreshToken refreshToken = null;

            if (!findTokenResult.IsNullResult())
            {
                refreshToken = await GenerateTokenWithouCollisionAsync(request, user, cancellationToken);
                await refreshTokenStorage.RevokeRefreshTokenAsync(findTokenResult.Value, refreshToken, "Re-authenticate", request.Ip, cancellationToken);
            }

            else
            {
                refreshToken = await GenerateTokenWithouCollisionAsync(request, user, cancellationToken);
                await refreshTokenStorage.CreateRefreshTokenAsync(refreshToken!, cancellationToken);
            }

            await refreshTokenStorage.RemoveAllExistsTokensAsync(user.Id, cancellationToken);

            return new AuthenticateUserModel(jwtTokenResult.Value, refreshToken!.Token);
        }

        private async ValueTask<Entities.RefreshToken> GenerateTokenWithouCollisionAsync(AuthenticateUserRequest request, User user, CancellationToken cancellationToken)
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
