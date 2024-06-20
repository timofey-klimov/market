namespace Auth.Domain.Exceptions
{
    public class BadRequestException : Exception
    {
        public BadRequestException(string message)
            : base(message)
        {

        }

        public static void Throw(string message) => throw new BadRequestException(message);
    }
}
