namespace Auth.Domain
{
    public class TokenSettings
    {
        public string Secret { get; set; }
        public int TokenTTL { get; set; }

        public int RefreshTokenTTL { get; set; }
    }
}
