namespace Auth.Domain.Exceptions
{
    public static class AppExceptions
    {
        public static Exception Domain(string message) => new DomainException(message);

        public static Exception Domain(IEnumerable<string> errors) => Domain(string.Join(", ", errors));

        public static Exception BadRequest(string message) => new BadRequestException(message);

        public static Exception Unauthorized() => new UnauthorizedException();
    }
}
