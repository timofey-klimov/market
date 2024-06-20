namespace Auth.Domain.Services.Security
{
    public interface ISecurityManager
    {
        public bool Verify(string userPassword, string passwordToVerify, string salt);

        public (string Password, string Salt) Hash(string password);
    }
}
