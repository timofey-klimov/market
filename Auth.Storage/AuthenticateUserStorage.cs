using Auth.Domain;
using Auth.Domain.Entities;
using Auth.Domain.Shared;
using Auth.Domain.Storage;
using Auth.Storage.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth.Storage
{
    public class AuthenticateUserStorage(AuthContext context) : IAuthenticateUserStorage
    {
        public async Task CreateUserAsync(Domain.Entities.User user, Domain.Entities.RefreshToken refreshToken, CancellationToken cancellationToken)
        {
            var tokenModel = new Storage.Models.RefreshToken
            {
                Id = refreshToken.Id,
                UserId = refreshToken.UserId,
                Token = refreshToken.Token,
                ExpiresAt = refreshToken.ExpiresAt,
                CreateByIp = refreshToken.CreateByIp,
            };

            var userToCreate = new Storage.Models.User
            {
                Id = user.Id,
                Login = user.Login,
                Password = user.Password,
                Salt = user.Salt,
                UserType = user.UserType switch
                {
                    Domain.Entities.UserType.Seller => Models.UserType.Seller,
                    Domain.Entities.UserType.Customer => Models.UserType.Customer,
                    Domain.Entities.UserType.PickupPoint => Models.UserType.PickupPoint,
                    _ => throw new InvalidOperationException()
                },
                IsActive = user.IsActive,
                Tokens = new List<Models.RefreshToken>() { tokenModel }
            };

            context.Users.Add(userToCreate);
            await context.SaveChangesAsync(cancellationToken);
           
        }

        public async Task<IResult<Domain.Entities.User>> GetUserByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            Domain.Entities.UserType Map(Models.UserType userType)
            {
                return userType switch
                {
                    Models.UserType.Seller => Domain.Entities.UserType.Seller,
                    Models.UserType.Customer => Domain.Entities.UserType.Customer,
                    Models.UserType.PickupPoint => Domain.Entities.UserType.PickupPoint,
                    _ => throw new NotImplementedException(),
                };
            }
            return user == null
                ? NullResult<Domain.Entities.User>.Create()
                : Domain.Entities.User.Create(user.Id, user.Login, user.Password, user.Salt, Map(user.UserType));
        }

        public async Task<IResult<Domain.Entities.User>> GetUserByLoginAsync(string login, CancellationToken token)
        {
            var user = await context.Users.AsNoTracking().SingleOrDefaultAsync(x => x.Login == login, token);
            Domain.Entities.UserType Map(Models.UserType userType)
            {
                return userType switch
                {
                    Models.UserType.Seller => Domain.Entities.UserType.Seller,
                    Models.UserType.Customer => Domain.Entities.UserType.Customer,
                    Models.UserType.PickupPoint => Domain.Entities.UserType.PickupPoint,
                    _ => throw new NotImplementedException(),
                };
            }
            return user == null
                ? NullResult<Domain.Entities.User>.Create()
                : Domain.Entities.User.Create(user.Id, user.Login, user.Password, user.Salt, Map(user.UserType));
        }

        public async Task<bool> IsLoginAvailableAsync(string login, CancellationToken cancellationToken)
        {
            return !(await context.Users.AnyAsync(x => x.Login == login, cancellationToken));
        }
    }
}
