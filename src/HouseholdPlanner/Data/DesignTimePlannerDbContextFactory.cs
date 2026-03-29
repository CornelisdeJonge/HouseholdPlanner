// File: src/HouseholdPlanner/Data/DesignTimePlannerDbContextFactory.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace HouseholdPlanner.Data
{
    public class DesignTimePlannerDbContextFactory : IDesignTimeDbContextFactory<PlannerDbContext>
    {
        public PlannerDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables();

            var configuration = configBuilder.Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DefaultConnection' not found. Check appsettings.json or update DesignTimePlannerDbContextFactory.");
            }

            var optionsBuilder = new DbContextOptionsBuilder<PlannerDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new PlannerDbContext(optionsBuilder.Options);
        }
    }
}