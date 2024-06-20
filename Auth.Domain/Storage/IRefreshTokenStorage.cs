using Auth.Domain.Entities;
using Auth.Domain.Shared;

namespace Auth.Domain.Storage
{
    public interface IRefreshTokenStorage
    {
        public Task<bool> ExistsAsync(Guid userId, string token, CancellationToken cancellationToken);
        public Task RemoveAllExistsTokensAsync(Guid userId, CancellationToken cancellationToken);

        public Task CreateRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

        public Task<IResult<RefreshToken>> FindAnyValidRefreshTokenAsync(Guid userId, CancellationToken cancellationToken);

        public Task<IResult<RefreshToken>> FindRefreshTokenAsync(Guid userId, string token, CancellationToken cancellationToken);

        public Task RevokeRefreshTokenAsync(RefreshToken revokeToken, RefreshToken newToken, string reason, string ip, CancellationToken cancellationToken);
    }
}
