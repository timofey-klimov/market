using MediatR;

namespace Auth.Domain.UseCases.CreateUser
{
    public record CreateUserRequest(string Login, string Password, int UserType, string Ip) : IRequest<AuthenticateUserModel>;
    
}
