using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NexOrder.AuthService.Infrastructure.Services
{
    
    public class AuthenticationService : IAuthenticationService
    {
        public string IssueJWT(string user)
        {
            var authSecret = Environment.GetEnvironmentVariable("AuthSecret");
            var audience = Environment.GetEnvironmentVariable("Audience");
            var issuer = Environment.GetEnvironmentVariable("Issuer");
            var expirationMinutes = Environment.GetEnvironmentVariable("ExpirationMinutes");
            var key = Encoding.UTF8.GetBytes(authSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("username", user),
                    new Claim("role", "User")
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt16(expirationMinutes)),
                Audience = audience,
                Issuer = issuer,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);
            return jwt;
        }
    }
}
