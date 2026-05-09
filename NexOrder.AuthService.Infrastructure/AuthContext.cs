using Microsoft.EntityFrameworkCore;
using NexOrder.AuthService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.AuthService.Infrastructure
{
    public class AuthContext : DbContext
    {
        public AuthContext(DbContextOptions<AuthContext> options) : base(options)
        {
            
        }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.HasDefaultSchema("NexOrder.Auth");
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(AuthContext).Assembly);
        }
    }
}
