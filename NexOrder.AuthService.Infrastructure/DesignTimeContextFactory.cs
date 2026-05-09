using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.AuthService.Infrastructure
{
    public class DesignTimeContextFactory : IDesignTimeDbContextFactory<AuthContext>
    {
        public AuthContext CreateDbContext(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AuthContext>();
            var connectionString = configuration.GetConnectionString("SystemDbConnectionString");

            // Explicitly set the migrations assembly
            optionsBuilder.UseSqlServer(
                connectionString,
                b => b.MigrationsAssembly("NexOrder.AuthService.Infrastructure")
            );

            return new AuthContext(optionsBuilder.Options);
        }
    }
}
