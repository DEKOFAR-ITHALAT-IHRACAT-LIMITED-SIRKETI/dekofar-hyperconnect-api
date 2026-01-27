using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Dekofar.HyperConnect.Infrastructure.Persistence
{
    public class DesignTimeDbContextFactory
        : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var environment =
                Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
                ?? "Development";

            var basePath = Directory.GetCurrentDirectory();
            basePath = ResolveBasePath(basePath);

            Console.WriteLine($"📁 BasePath: {basePath}");
            Console.WriteLine($"🌍 Environment: {environment}");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString =
                configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found.");

            var optionsBuilder =
                new DbContextOptionsBuilder<ApplicationDbContext>();

            optionsBuilder.UseNpgsql(connectionString);

            return new ApplicationDbContext(optionsBuilder.Options);
        }

        private static string ResolveBasePath(string basePath)
        {
            var appSettingsPath = Path.Combine(basePath, "appsettings.json");
            if (File.Exists(appSettingsPath))
            {
                return basePath;
            }

            var apiPath = Path.Combine(basePath, "dekofar-hyperconnect-api");
            if (File.Exists(Path.Combine(apiPath, "appsettings.json")))
            {
                return apiPath;
            }

            var parent = Directory.GetParent(basePath);
            if (parent != null)
            {
                var parentApiPath = Path.Combine(parent.FullName, "dekofar-hyperconnect-api");
                if (File.Exists(Path.Combine(parentApiPath, "appsettings.json")))
                {
                    return parentApiPath;
                }
            }

            return basePath;
        }
    }
}
