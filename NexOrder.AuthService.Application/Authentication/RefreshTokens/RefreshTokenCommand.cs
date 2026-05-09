using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.AuthService.Application.Authentication.RefreshTokens
{
    public record RefreshTokenCommand(string Token, string RefreshToken);
}
