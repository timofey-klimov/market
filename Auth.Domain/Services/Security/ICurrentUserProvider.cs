namespace Auth.Domain.Services.Security
{
    public interface ICurrentUserProvider
    {
        Guid UserId { get; }
    }
}
