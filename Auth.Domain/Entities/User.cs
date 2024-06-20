using Auth.Domain.Entities;
using Auth.Domain.Shared;

namespace Auth.Domain.Entities
{
    public enum UserType
    {
        Customer = 0,
        Seller = 1,
        PickupPoint = 2
    }

    public class User
    {
        public Guid Id { get; private set; }

        public string Login { get; private set; }

        public string Password { get; private set; }

        public string Salt { get; private set; }

        public bool IsActive { get; private set; }

        public UserType UserType { get; private set; }


        public IEnumerable<RefreshToken> Tokens { get; private set; }

        protected User() { }
        private User(Guid id, string login, string password, string salt, UserType userType)
        {
            Id = id;
            Login = login;
            Password = password;
            Salt = salt;
            UserType = userType;
            IsActive = true;
        }

        public static IResult<User> Create(Guid id, string login, string password, string salt, UserType userType)
        {
            return Result<User>
                .Bind(() => new User(id, login, password, salt, userType))
                .Validate(() => id != Guid.Empty, "Invalid id")
                .Validate(login.IsNotEmpty, "Login is empty")
                .Validate(password.IsNotEmpty, "Password is empty")
                .Validate(salt.IsNotEmpty, "Salt is empty");
        }


        public override bool Equals(object? obj)
        {
            return obj is User user && user.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

    }
}
