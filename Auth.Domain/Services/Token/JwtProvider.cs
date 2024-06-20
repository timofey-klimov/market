using Auth.Domain;
using Auth.Domain.Entities;
using Auth.Domain.Shared;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Auth.Domain.Services.Token
{
    public class JwtProvider(IOptions<TokenSettings> settings) : IJwtProvider
    {
        public IResult<string> GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(settings.Value.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddHours(settings.Value.TokenTTL),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return Result<string>
                .Bind(() => tokenString)
                .Validate(tokenString.IsNotEmpty, "Token is invalid");
        }

        public IResult<RefreshToken> GenerateRefreshToken(Guid id, Guid userId, string ipAddress)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return RefreshToken.Create(id, userId, token, DateTime.UtcNow.AddHours(settings.Value.RefreshTokenTTL), ipAddress);
        }
    }
}
