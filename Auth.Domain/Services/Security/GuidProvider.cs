namespace Auth.Domain.Services.Security
{
    public class GuidProvider : IGuidProvider
    {
        public Guid New() => Guid.NewGuid();
    }
}
