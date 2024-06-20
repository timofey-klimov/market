using Auth.Domain.Entities;
using Auth.Domain.Shared;

namespace Auth.Domain.Storage
{
    public interface IAuthenticateUserStorage
    {
        Task<IResult<User>> GetUserByLoginAsync(string login, CancellationToken token);

        Task<bool> IsLoginAvailableAsync(string login, CancellationToken cancellationToken);

        Task CreateUserAsync(User user, RefreshToken token, CancellationToken cancellationToken);

        Task<IResult<User>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
