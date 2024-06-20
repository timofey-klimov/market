using Auth.Domain.Entities;
using Auth.Domain.Exceptions;
using Auth.Domain.Services.Security;
using Auth.Domain.Services.Token;
using Auth.Domain.Shared;
using Auth.Domain.Storage;
using Auth.Domain.Storage.Transaction;
using MediatR;

namespace Auth.Domain.UseCases.CreateUser
{
    public class CreateUserHandler(
        IRefreshTokenStorage refreshTokenStorage,
        ISecurityManager securityManager,
        IGuidProvider guidProvider,
        IAuthenticateUserStorage authenticateUserStorage,
        IJwtProvider jwtProvider) : IRequestHandler<CreateUserRequest, AuthenticateUserModel>
    {
        public async Task<AuthenticateUserModel> Handle(CreateUserRequest request, CancellationToken cancellationToken)
        {
            if (request.Password.IsEmpty())
                BadRequestException.Throw("Invalid password");

            if (!await authenticateUserStorage.IsLoginAvailableAsync(request.Login, cancellationToken))
                BadRequestException.Throw("Login is not available");

            var (hash, salt) = securityManager.Hash(request.Password);
            var userResult = User.Create(guidProvider.New(), request.Login, hash, salt, (UserType)request.UserType);
            userResult.Handle(
                handleError: AppExceptions.Domain);

            var tokenResult = jwtProvider.GenerateJwtToken(userResult.Value);
            tokenResult.Handle(handleError: AppExceptions.Domain);
            var refreshTokenResult = jwtProvider.GenerateRefreshToken(guidProvider.New(), userResult.Value.Id, request.Ip);
            refreshTokenResult.Handle(handleError: AppExceptions.Domain);
            await authenticateUserStorage.CreateUserAsync(userResult.Value, refreshTokenResult.Value, cancellationToken);
                  
            return new AuthenticateUserModel(tokenResult.Value, refreshTokenResult.Value.Token);
        }
    }
}
