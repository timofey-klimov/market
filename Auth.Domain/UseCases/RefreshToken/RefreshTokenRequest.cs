using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Auth.Domain.UseCases.RefreshToken
{
    public record RefreshTokenRequest(string RefreshToken, string Ip) : IRequest<AuthenticateUserModel>;
}
