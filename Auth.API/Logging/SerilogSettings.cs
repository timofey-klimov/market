using MediatR;
using Serilog;
using Serilog.Events;
using Serilog.Filters;

namespace Auth.API.Logging
{
    public static class SerilogSettings
    {
        public static WebApplicationBuilder AddLogs(this WebApplicationBuilder builder)
        {
            builder.Logging.ClearProviders();
            var loggerConfiguration = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
                .Enrich.WithProperty("Application", "Auth.Api")
                .WriteTo.Logger(lc => lc
                    .WriteTo.Console())
                .WriteTo.Logger(lc => lc
                    .Filter
                    .ByExcluding(Matching.FromSource("Microsoft"))
                    .WriteTo.OpenSearch(new Serilog.Sinks.OpenSearch.OpenSearchSinkOptions
                    {
                        IndexFormat = "auth-logs-{0:yyyy.MM.dd}"
                    }));
            builder.Logging.AddSerilog(loggerConfiguration.CreateLogger());
            Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine(msg));
            builder.Services
                .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            return builder;
        }
    }
}
