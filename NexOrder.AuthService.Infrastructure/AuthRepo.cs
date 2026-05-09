using NexOrder.AuthService.Application;
using NexOrder.AuthService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.AuthService.Infrastructure
{
    public class AuthRepo : IAuthRepo
    {
        private readonly AuthContext context;
        public AuthRepo(AuthContext context)
        {
            this.context = context;
        }

        public IQueryable<RefreshToken> GetRefreshTokens()
        {
            return this.context.RefreshTokens.AsQueryable();
        }

        public async Task SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            if(refreshToken.Id == 0)
            {
                this.context.RefreshTokens.Add(refreshToken);
            }
            else
            {
                this.context.Entry(refreshToken).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            }

            await this.context.SaveChangesAsync();
        }
    }
}
