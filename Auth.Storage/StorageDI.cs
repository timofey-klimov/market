using Auth.Domain.Storage;
using Auth.Domain.Storage.Transaction;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Storage
{
    public static class StorageDI
    {
        public static IServiceCollection AddStorage(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticateUserStorage, AuthenticateUserStorage>();
            services.AddScoped<IRefreshTokenStorage, RefreshTokenStorage>();
            services.AddScoped<ITransactionProvider, TransactionProvider>();
            return services;
        }
    }
}
