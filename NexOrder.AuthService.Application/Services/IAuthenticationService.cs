using NexOrder.AuthService.Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.AuthService.Application.Services
{
    public interface IAuthenticationService
    {
        Task<RefreshTokenResponse> GenerateNewTokenAsync(Guid userId, string email);

        Task<(RefreshTokenResponse? RefreshToken, string ErrorMessage)> GenerateJwtAndRefreshTokenAsync(string accessToken, string refreshToken);
    }
}
