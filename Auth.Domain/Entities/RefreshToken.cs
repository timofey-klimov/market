using Auth.Domain.Shared;

namespace Auth.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; private set; }

        public Guid UserId { get; private set; }

        public string Token { get; private set; }

        public DateTime ExpiresAt { get; private set; }

        public string CreateByIp { get; private set; }

        public DateTime? RevokeAt { get; private set; }
        public string RevokedByIp { get; private set; }
        public string ReplacedByToken { get; private set; }
        public string ReasonRevoked { get; private set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsRevoked => RevokeAt != null;
        public bool IsActive => !IsRevoked && !IsExpired;

        private RefreshToken() { }

        private RefreshToken(Guid id, Guid userId, string token, DateTime expiresAt, string createByIp)
        {
            Id = id;
            UserId = userId;
            Token = token;
            ExpiresAt = expiresAt;
            CreateByIp = createByIp;
        }

        public static IResult<RefreshToken> Create(Guid id, Guid userId, string token, DateTime expiresAt, string createByIp)
        {
            return Result<RefreshToken>
                .Bind(() => new RefreshToken(id, userId, token, expiresAt, createByIp))
                .Validate(() => userId != Guid.Empty, "UserId is invalid")
                .Validate(() => id != Guid.Empty, "Id is invalid")
                .Validate(token.IsNotEmpty, "Token is empty")
                .Validate(() => expiresAt > DateTime.UtcNow, "ExpiresAt is invalid")
                .Validate(createByIp.IsNotEmpty, "CreateByIp is empty");
        }

        public RefreshToken Revoke(string replacedByToken, string reason, string ip)
        {
            return new RefreshToken()
            {
                Id = this.Id,
                UserId = this.UserId,
                Token = this.Token,
                ExpiresAt = this.ExpiresAt,
                CreateByIp = this.CreateByIp,
                RevokeAt = DateTime.UtcNow,
                RevokedByIp = ip,
                ReasonRevoked = reason,
                ReplacedByToken = replacedByToken
            };
        }
    }
}
