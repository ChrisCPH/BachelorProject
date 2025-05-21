using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using RunningPlanner.Data;
using Microsoft.AspNetCore.Hosting;
using MongoDB.Driver;

namespace RunningPlanner.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove ALL existing DbContext configurations
                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<RunningPlannerDbContext>) ||
                              d.ServiceType == typeof(RunningPlannerDbContext))
                    .ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // Create a fresh service provider for InMemory
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                // Add DbContext with InMemory and isolated provider
                services.AddDbContext<RunningPlannerDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                    options.UseInternalServiceProvider(serviceProvider);
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                });

                // Build the provider and ensure database is created
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<RunningPlannerDbContext>();
                    db.Database.EnsureCreated();
                }
            });
        }
    }
}