using Auth.Domain.Entities;
using Auth.Domain.Shared;
using Auth.Domain.Storage;
using Auth.Storage.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth.Storage
{
    public class RefreshTokenStorage(AuthContext context) : IRefreshTokenStorage
    {
        public async Task CreateRefreshTokenAsync(Domain.Entities.RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            var tokenModel = new Storage.Models.RefreshToken
            {
                Id = refreshToken.Id,
                UserId = refreshToken.UserId,
                Token = refreshToken.Token,
                ExpiresAt = refreshToken.ExpiresAt,
                CreateByIp = refreshToken.CreateByIp,
            };
            context.RefreshTokens.Add(tokenModel);
            await context.SaveChangesAsync(cancellationToken);
        }

        public Task<bool> ExistsAsync(Guid userId, string token, CancellationToken cancellationToken)
        {
            return context.RefreshTokens.AsNoTracking().AnyAsync(x => x.Token == token && x.UserId == userId);
        }

        public async Task<IResult<Domain.Entities.RefreshToken>> FindAnyValidRefreshTokenAsync(Guid userId, CancellationToken cancellationToken)
        {
            var tokenModel = await context.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(
                x => x.UserId == userId && x.ReplacedByToken == null && x.ExpiresAt > DateTime.UtcNow, cancellationToken);
            return tokenModel == null
                ? NullResult<Domain.Entities.RefreshToken>.Create()
                : Domain.Entities.RefreshToken.Create(tokenModel.Id, tokenModel.UserId, tokenModel.Token, tokenModel.ExpiresAt, tokenModel.CreateByIp);
        }

        public async Task<IResult<Domain.Entities.RefreshToken>> FindRefreshTokenAsync(Guid userId, string token, CancellationToken cancellationToken)
        {
            var refreshToken = await context.RefreshTokens.FirstOrDefaultAsync(
                x => x.UserId == userId && x.Token == token && x.ExpiresAt > DateTime.UtcNow && x.RevokeAt == null);

            return refreshToken == null
                ? NullResult<Domain.Entities.RefreshToken>.Create()
                : Domain.Entities.RefreshToken.Create(refreshToken.Id, refreshToken.UserId, refreshToken.Token, refreshToken.ExpiresAt, refreshToken.CreateByIp);
        }

        public async Task RemoveAllExistsTokensAsync(Guid userId, CancellationToken cancellationToken)
        {
            var tokens = context.RefreshTokens.Where(x => x.UserId == userId && x.ExpiresAt < DateTime.UtcNow).ToArray();

            context.RefreshTokens.RemoveRange(tokens);
            await context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IResult<Domain.Entities.RefreshToken>> RevokeRefreshTokenAsync(
            Domain.Entities.RefreshToken revokeToken, Domain.Entities.RefreshToken newToken, string reason, string ip, CancellationToken cancellationToken)
        {
            await CreateRefreshTokenAsync(newToken, cancellationToken);
            var revokeTokenResult = revokeToken.Revoke(newToken.Token, reason, ip);
            if (!revokeTokenResult.Errors.Any())
                await UpdateAsync(revokeTokenResult.Value, cancellationToken);
            return revokeTokenResult;

        }

        private async Task UpdateAsync(Domain.Entities.RefreshToken refreshToken, CancellationToken cancellation)
        {
            var tokenModel = new Storage.Models.RefreshToken
            {
                Id = refreshToken.Id,
                UserId = refreshToken.UserId,
                Token = refreshToken.Token,
                ExpiresAt = refreshToken.ExpiresAt,
                CreateByIp = refreshToken.CreateByIp,
                ReasonRevoked = refreshToken.ReasonRevoked,
                ReplacedByToken = refreshToken.ReplacedByToken,
                RevokeAt = refreshToken.RevokeAt,
                RevokedByIp = refreshToken.RevokedByIp
            };

            context.RefreshTokens.Update(tokenModel);
            await context.SaveChangesAsync(cancellation);
        }

    }
}
