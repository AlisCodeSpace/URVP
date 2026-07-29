using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using RICHConnect.Backend.Infrastructure.Data;

namespace RICHConnect.Backend.Api.Configuration.Database
{
    /// <summary>
    /// Configuration for database services
    /// </summary>
    public static class DatabaseConfiguration
    {
        /// <summary>
        /// Configure database context with the appropriate provider
        /// </summary>
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("SqlServerConnection");

            // Register AppDbContext with SQL Server
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Configure Data Protection to persist keys in the database
            // This ensures keys survive application restarts and are shared across multiple server instances
            services.AddDataProtection()
                .SetApplicationName("RICHConnect.Backend") // Ensure consistent key isolation
                .PersistKeysToDbContext<AppDbContext>();

            return services;
        }
    }
}
