using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace RICHConnect.Backend.Infrastructure.Data
{
    /// <summary>
    /// Design-time factory for AppDbContext to support migrations
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Determine environment (default to Development for design-time)
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") 
                ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") 
                ?? "Development";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddUserSecrets<AppDbContextFactory>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            
            var connectionString = configuration.GetConnectionString("SqlServerConnection");
                
            if (connectionString == null)
            {
                throw new InvalidOperationException(
                    "Connection string not found in configuration. " +
                    "Set it via user-secrets (dotnet user-secrets set \"ConnectionStrings:SqlServerConnection\" \"<value>\") " +
                    "or environment variable (ConnectionStrings__SqlServerConnection).");
            }
                
            // Use SQL Server
            optionsBuilder.UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
} 