using MediatR;

namespace Auth.Domain.UseCases.AuthorizeUser
{
    public record AuthenticateUserRequest(string Login, string Password, string Ip) : IRequest<AuthenticateUserModel>;
}
