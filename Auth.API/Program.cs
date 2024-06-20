using Auth.API;
using Auth.API.Dependencies;
using Auth.API.Endpoints;
using Auth.API.Logging;
using Auth.API.OpenTelemetry;
using Auth.API.Swagger;
using Auth.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();

builder.Services.AddMediatR(opts =>
{
    opts.RegisterServicesFromAssemblies(Assembly.Load("Auth.Domain"));
})
.AddDbContextPool<AuthContext>(opts =>
 {
     opts.UseNpgsql(builder.Configuration.GetConnectionString("Postgre"));
 })
           
.AddStorage()
.AddSwagger()
.AddDomain()
.AddSettings()
.AddAuthorization()
.AddAuthentication("jwt")
.AddJwtBearer("jwt", o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = false,
        ValidateIssuer = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["TokenSettings:Secret"])),
        ValidateIssuerSigningKey = true,
    };
});
builder.AddLogs();
var app = builder.Build();

app.UseMiddleware<ExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(opts => opts.SerializeAsV2 = true);

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Employee API V1");
        c.RoutePrefix = string.Empty;
    });
}
app.UseAuthentication();
app.UseAuthorization();
app.MapAccountEndpoint();
app.MapPrometheusScrapingEndpoint();

app.Run();
