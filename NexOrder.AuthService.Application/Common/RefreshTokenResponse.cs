using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.AuthService.Application.Common
{
    public record RefreshTokenResponse(string AccessToken, string RefreshToken);
}
