namespace Auth.API.Contracts
{
    public class CreateUserModel
    {
        public string Login { get; set; }

        public string Password { get; set; }

        public int UserType { get; set; }
    }
}
