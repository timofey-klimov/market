using Auth.Domain.Entities;
using Auth.Domain.Shared;

namespace Auth.Domain.Services.Token
{
    public interface IJwtProvider
    {
        public IResult<string> GenerateJwtToken(User user);
        public IResult<RefreshToken> GenerateRefreshToken(Guid id, Guid userId, string ipAddress);
    }
}
