using Auth.Domain.Services.Security;
using Auth.Domain.Services.Token;

namespace Auth.API.Dependencies
{
    public static class DomainDI
    {
        public static IServiceCollection AddDomain(this IServiceCollection services)
        {
            services.AddScoped<IGuidProvider, GuidProvider>();
            services.AddScoped<ISecurityManager, SecurityManager>();
            services.AddScoped<IJwtProvider, JwtProvider>();
            services.AddScoped<ICurrentUserProvider, UserProvider>();
            services.AddHttpContextAccessor();
            return services;
        }
    }
}
