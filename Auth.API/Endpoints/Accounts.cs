using Auth.API.Contracts;
using Auth.Domain.UseCases.AuthorizeUser;
using Auth.Domain.UseCases.CreateUser;
using Auth.Domain.UseCases.RefreshToken;
using MediatR;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Auth.API.Endpoints
{
    public static class Accounts
    {
        public static void MapAccountEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/accounts/sign-in", async (AuthenticateUserModel model, ISender sender, HttpContext context) =>
            {
                var result = await sender.Send(new AuthenticateUserRequest(model.Login, model.Password, context.GetIpAddress()));
                context.Response.Cookies.Append("RefreshToken", result.RefreshToken, new CookieOptions { HttpOnly = true, Domain="Freshmark"});
                return result.Token;
            });

            app.MapPost("/accounts/sign-up", async (CreateUserModel model, ISender sender, HttpContext context) =>
            {
                var result = await sender.Send(new CreateUserRequest(model.Login, model.Password, model.UserType, context.GetIpAddress()));
                context.Response.Cookies.Append("RefreshToken", result.RefreshToken, new CookieOptions { HttpOnly = true, Domain = "Freshmark" });
                return result.Token;
            });

            app.MapPost("/accounts/refresh-token", async (ISender sender, HttpContext context) =>
            {
                if (!context.Request.Cookies.TryGetValue("RefreshToken", out var token))
                    throw new Domain.Exceptions.UnauthorizedException();

                var result = await sender.Send(new RefreshTokenRequest(token, context.GetIpAddress()));
                context.Response.Cookies.Append("RefreshToken", result.RefreshToken, new CookieOptions { HttpOnly = true, Domain = "Freshmark" });
                return result.Token;
            }).RequireAuthorization();
        }
    }
}
