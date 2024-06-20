using OpenTelemetry.Metrics;

namespace Auth.API.OpenTelemetry
{
    public static class AppTelemetry
    {
        public static void AddOpenTelemetry(this WebApplicationBuilder builder)
        {
            builder.Services.AddOpenTelemetry()
                .WithMetrics(opts =>
                {
                    opts.AddAspNetCoreInstrumentation();
                    opts.AddPrometheusExporter();
                });
        }
    }
}
