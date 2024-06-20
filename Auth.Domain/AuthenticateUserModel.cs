namespace Auth.Domain
{
    public record AuthenticateUserModel(string Token, string RefreshToken);
}
