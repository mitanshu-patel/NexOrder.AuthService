using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NexOrder.AuthService.Application;
using NexOrder.AuthService.Application.Common;
using NexOrder.AuthService.Application.Services;
using NexOrder.AuthService.Domain;
using NexOrder.AuthService.Shared.EncryptionDecryption;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NexOrder.AuthService.Infrastructure.Services
{
    
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthRepo authRepo;
        private readonly IEncryptionDecryptionService encryptionDecryptionService;
        private readonly string authSecret;
        private readonly string issuer;
        private readonly string audience;
        private readonly int accessTokenExpirationMinutes;
        private readonly int refreshTokenExpirationMinutes;

        public AuthenticationService(IAuthRepo authRepo, IEncryptionDecryptionService encryptionDecryptionService)
        {
            this.authRepo = authRepo;
            this.encryptionDecryptionService = encryptionDecryptionService;
            this.authSecret = Environment.GetEnvironmentVariable("AuthSecret") ?? string.Empty;
            this.audience = Environment.GetEnvironmentVariable("Audience") ?? string.Empty;
            this.issuer = Environment.GetEnvironmentVariable("Issuer") ?? string.Empty;
            this.accessTokenExpirationMinutes = Convert.ToInt16(Environment.GetEnvironmentVariable("ExpirationMinutes"));
            this.refreshTokenExpirationMinutes = Convert.ToInt16(Environment.GetEnvironmentVariable("RefreshTokenExpirationMinutes"));
        }
        public async Task<(RefreshTokenResponse? RefreshToken, string ErrorMessage)> GenerateJwtAndRefreshTokenAsync(string accessToken, string refreshToken)
        {
            var accessTokenClaims = this.GetClaims(accessToken);
            if(accessTokenClaims.Count == 0)
            {
                return (null, "Invalid Access Token");
            }

            var refreshTokenClaims = this.GetClaims(refreshToken);
            if (refreshTokenClaims.Count == 0)
            {
                return (null, "Invalid Refresh Access Token");
            }

            var areClaimsMatched = refreshTokenClaims.All(v =>
                                        accessTokenClaims.Any(t => t.Type == v.Type &&
                                             t.Value == v.Value));
            if (!areClaimsMatched)
            {
                return (null, "Claims not matched");
            }

            var userId = refreshTokenClaims.FirstOrDefault(v => v.Type == "userId")?.Value ?? string.Empty;
            var userGuid = Guid.Parse(userId);
            var email = refreshTokenClaims.FirstOrDefault(v => v.Type == "username")?.Value ?? string.Empty;
            var jti = this.GetJti(accessToken);
            var refreshTokenDetail = await this.authRepo.GetRefreshTokens()
                                                        .FirstOrDefaultAsync(v =>
                                                        v.JwtId.Equals(jti) &&
                                                        v.UserId == userGuid &&
                                                        v.Email == email);

            if(refreshTokenDetail == null)
            {
                return (null, "Refresh Token not matched");
            }

            if (refreshTokenDetail.Invalidated)
                return (null, "Refresh Token already expired, try generating new access token");

            // invalidate the current refresh token in database as we are going to generate new refresh token and return it to user
            refreshTokenDetail.Invalidated = true;
            await this.authRepo.SaveRefreshTokenAsync(refreshTokenDetail);

            if (refreshTokenDetail.ExpiryDate <= DateTime.Now)
            {
                return (null, "Refresh Token already expired, try generating new access token");
            }

            // invalidate the current refresh token in database as we are going to generate new refresh token and return it to user
            refreshTokenDetail.Invalidated = true;
            await this.authRepo.SaveRefreshTokenAsync(refreshTokenDetail);

            var (newAccessToken, newRefreshToken) = await this.GenerateNewTokenAsync(userGuid, email);

            return (new(newAccessToken, newRefreshToken), string.Empty);
        }

        public async Task<RefreshTokenResponse> GenerateNewTokenAsync(Guid userId, string email)
        {
           
            var (tokenId, accessToken) = this.IssueJWT(email, userId, accessTokenExpirationMinutes);
            var (_, refreshTokenValue) = this.IssueJWT(email, userId, refreshTokenExpirationMinutes);
            var encryptedTokenValue = this.encryptionDecryptionService.EncryptSecret(refreshTokenValue);
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                ExpiryDate = DateTime.Now.AddMinutes(this.refreshTokenExpirationMinutes),
                Token = encryptedTokenValue,
                Invalidated = false,
                JwtId = tokenId,
                Email = email
            };

            await this.authRepo.SaveRefreshTokenAsync(refreshToken);
            return new(accessToken, refreshTokenValue);
        }

        private List<Claim> GetClaims(string token)
        {
            var key = Encoding.UTF8.GetBytes(this.authSecret);
            var tokenHandler = new JwtSecurityTokenHandler();
            var claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = this.issuer,
                ValidAudience = this.audience,
                IssuerSigningKey = new SymmetricSecurityKey(key)
            }, out _);

            return claimsPrincipal?.Claims.Where(v => v.Type== "username" || v.Type == "userId").ToList() ?? [];
        }

        private (string jwtId, string token) IssueJWT(string user, Guid userId, int expirationMinutes)
        {
            var key = Encoding.UTF8.GetBytes(this.authSecret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("username", user),
                    new Claim("role", "User"),
                    new Claim("userId", userId.ToString()),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                }),
                Expires = DateTime.UtcNow.AddMinutes(Convert.ToInt16(expirationMinutes)),
                Audience = this.audience,
                Issuer = this.issuer,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);
            var jwtId = tokenHandler.ReadJwtToken(jwt).Id;
            return (jwtId, jwt);
        }

        private string GetJti(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.ReadJwtToken(token).Id;
        }

        public string IssueJWT(Guid userId, int expirationMinutes)
        {
            throw new NotImplementedException();
        }
    }
}
