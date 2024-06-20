using Auth.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Auth.API
{
    public class ExceptionHandlerMiddleware(RequestDelegate next)
    {
        public async Task Invoke(HttpContext context)
        {
            var logger = context.RequestServices.GetRequiredService<ILogger<ExceptionHandlerMiddleware>>();
            try
            {
                logger.LogInformation("Start handle path {Path}", context.Request.Path.Value);
                await next(context);
            }
            catch (Exception ex)
            {

                var problemDetails = ex switch
                {
                    BadRequestException badRequest => new ProblemDetails()
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Type = "Bad Request",
                        Detail = badRequest.Message
                    },
                    UnauthorizedException unauthorized => new ProblemDetails
                    {
                        Status = StatusCodes.Status401Unauthorized,
                        Type = "Unauthorized",
                        Detail = "Invalid token or credentials"
                    },
                    _ => new ProblemDetails()
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Type = "Internal Server Error",
                        Detail = "Error occured"
                    }
                };

                logger.LogError("Error in path {Path} with {Message}", context.Request.Path.Value, ex.Message);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = problemDetails.Status!.Value;
                await context.Response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}
