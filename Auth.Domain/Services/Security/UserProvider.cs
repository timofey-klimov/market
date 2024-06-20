using Microsoft.AspNetCore.Http;

namespace Auth.Domain.Services.Security
{
    public class UserProvider(IHttpContextAccessor httpContextAccessor) : ICurrentUserProvider
    {
        public Guid UserId => GetUserId();
        

        private Guid GetUserId()
        {
            var user = httpContextAccessor.HttpContext.User;
            return Guid.Parse(user.Claims.First(x => x.Type == "id").Value);
        }
    }
}
