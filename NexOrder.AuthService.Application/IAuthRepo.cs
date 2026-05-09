using NexOrder.AuthService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.AuthService.Application
{
    public interface IAuthRepo
    {
        public IQueryable<RefreshToken> GetRefreshTokens();

        public Task SaveRefreshTokenAsync(RefreshToken refreshToken);
    }
}
