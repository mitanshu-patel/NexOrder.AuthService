using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NexOrder.AuthService.Application;
using NexOrder.AuthService.Application.Services;
using NexOrder.AuthService.Infrastructure;
using NexOrder.AuthService.Infrastructure.Helpers;
using NexOrder.AuthService.Infrastructure.Services;
using NexOrder.AuthService.Shared.EncryptionDecryption;
using NexOrder.Framework.Core;
using NexOrder.Framework.Core.Common;
using System.Reflection;

var builder = FunctionsApplication.CreateBuilder(args);

var configuration = new ConfigurationBuilder()
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
var environment = configuration.GetValue<string>("ENVIRONMENT");
var isDevelopment = !string.IsNullOrEmpty(environment) && environment.Equals(
            "DEVELOPMENT",
            System.StringComparison.InvariantCultureIgnoreCase);

builder.ConfigureFunctionsWebApplication();
var appInsightsConnection = Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING");

builder.Services.AddNexOrderCustomLogging(isDevelopment, "NexOrder.AuthService", appInsightsConnection);
builder.Services.AddMessageDeliveryService(options =>
{
    options.ServiceBusConnectionString = configuration["ServiceBusConnectionString"]
        ?? configuration.GetConnectionString("ServiceBusConnectionString")
        ?? string.Empty;
#if DEBUG
    options.WebProxyAddress = Environment.GetEnvironmentVariable("WebProxy") ?? string.Empty;
#endif
});

builder.Services.RegisterHandlers(Assembly.Load("NexOrder.AuthService.Application"));

var connectionString = ConnectionStringsHelper.GetDbConnectionString();
builder.Services.AddDbContext<AuthContext>(
    v => v.UseSqlServer(connectionString,
    b => b.MigrationsAssembly("NexOrder.AuthService.Infrastructure")));
builder.Services.AddScoped<IAuthRepo, AuthRepo>();

builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddSingleton<IEncryptionDecryptionService, EncryptionDecryptionService>();
builder.Services.Configure<EncryptionDecryptionServiceOptions>(configuration.GetSection("Encryption"));

var app = builder.Build();
if (builder.Configuration.GetValue<bool>("RunMigration"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AuthContext>();
    db.Database.Migrate();
    //return; // Exit after migration
}

app.Run();
