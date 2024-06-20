using Auth.Domain;

namespace Auth.API.Dependencies
{
    public static class ConfigurationDI
    {
        public static IServiceCollection AddSettings(this IServiceCollection services)
        {
            services
            .AddOptions<TokenSettings>()
            .BindConfiguration(nameof(TokenSettings))
            .ValidateDataAnnotations()
            .ValidateOnStart();

            return services;
        }
    }
}
