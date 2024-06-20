namespace Auth.Storage.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Token { get; set; }

        public DateTime ExpiresAt { get; set; }

        public string CreateByIp { get; set; }

        public DateTime? RevokeAt { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByToken { get; set; }
        public string? ReasonRevoked { get; set; }


        public RefreshToken() { }
        public RefreshToken(Guid id, Guid userId, string token, DateTime expiresAt, string createByIp)
        {
            Id = id;
            Token = token;
            ExpiresAt = expiresAt;
            CreateByIp = createByIp;
            UserId = userId;
        }
    }
}
