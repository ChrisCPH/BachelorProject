using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using RunningPlanner.Data;
using Microsoft.AspNetCore.Hosting;
using MongoDB.Driver;
using Mongo2Go;  // <-- Add this

namespace RunningPlanner.Tests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
    {
        private MongoDbRunner? _mongoRunner;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Start Mongo2Go server if not already started
                if (_mongoRunner == null)
                {
                    _mongoRunner = MongoDbRunner.Start();
                }

                var mongoDescriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(IMongoClient));
                if (mongoDescriptor != null)
                {
                    services.Remove(mongoDescriptor);
                }

                services.AddSingleton<IMongoClient>(sp =>
                {
                    return new MongoClient(_mongoRunner.ConnectionString);
                });

                var descriptors = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<RunningPlannerDbContext>) ||
                                d.ServiceType == typeof(RunningPlannerDbContext))
                    .ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                services.AddDbContext<RunningPlannerDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDb");
                    options.UseInternalServiceProvider(serviceProvider);
                    options.EnableDetailedErrors();
                    options.EnableSensitiveDataLogging();
                });

                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<RunningPlannerDbContext>();
                db.Database.EnsureCreated();
            });
        }

        public new void Dispose()
        {
            base.Dispose();
            _mongoRunner?.Dispose();
        }
    }
}
