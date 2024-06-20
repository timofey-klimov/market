using System.Security.Cryptography;
using System.Text;

namespace Auth.Domain.Services.Security
{
    public class SecurityManager : ISecurityManager
    {
        private Lazy<SHA256> _sha = new Lazy<SHA256>(SHA256.Create);
        public (string Password, string Salt) Hash(string password)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(32);
            var passwordBytes = Encoding.UTF8.GetBytes(password);
            return HashInternal(passwordBytes, saltBytes);
        }

        public bool Verify(string userPassword, string passwordToVerify, string salt)
        {
            var passwordBytes = Encoding.UTF8.GetBytes(passwordToVerify);
            var saltBytes = Convert.FromBase64String(salt);
            var hash = HashInternal(passwordBytes, saltBytes);
            return userPassword == hash.Password;
        }

        private (string Password, string Salt) HashInternal(byte[] password, byte[] salt)
        {
            var buffer = new byte[password.Length + salt.Length];
            password.CopyTo(buffer, 0);
            salt.CopyTo(buffer, password.Length);

            using (_sha.Value)
            {
                using (var ms = new MemoryStream(buffer))
                {
                    var hash = _sha.Value.ComputeHash(ms);
                    return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
                }
            }
        }
    }
}
