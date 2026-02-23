using CbTsSa_Shared.DBModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace CbTsSa_Shared.Models.Factories
{
    public class CommandBotDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Determine environment (default to Development)
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            // Load configuration from CommandBot project
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "..", "CommandBot"))
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .Build();

            // Default connection string (local dev or docker)
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            // Heroku DATABASE_URL override
            var herokuUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            if (!string.IsNullOrEmpty(herokuUrl))
            {
                var uri = new Uri(herokuUrl);
                var userInfo = uri.UserInfo.Split(':');

                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = uri.Host,
                    Port = uri.Port,
                    Username = userInfo[0],
                    Password = userInfo[1],
                    Database = uri.AbsolutePath.Trim('/'),
                    SslMode = SslMode.Require,
                    TrustServerCertificate = true
                };

                connectionString = builder.ConnectionString;
            }

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found. " +
                    "Check CommandBot/appsettings.json or set ConnectionStrings__DefaultConnection.");
            }

            // Build DbContext with PostgreSQL provider
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString, b => b.MigrationsAssembly("CbTsSa_Shared"));

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}