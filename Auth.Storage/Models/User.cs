using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Storage.Models
{
    public enum UserType : int
    {
        Customer = 0,
        Seller = 1,
        PickupPoint = 2
    }

    public class User
    {
        public Guid Id { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string Salt { get; set; }

        public bool IsActive { get; set; }

        public UserType UserType { get; set; }
        public IEnumerable<RefreshToken> Tokens { get; set; }

        public User() { }
        public User(Guid id, string login, string password, string salt, UserType userType)
        {
            Id = id;
            Login = login;
            Password = password;
            Salt = salt;
            UserType = userType;
        }
    }
}
