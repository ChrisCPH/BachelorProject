using Microsoft.Extensions.DependencyInjection;
using RunningPlanner.Data;

namespace RunningPlanner.Tests
{
    public class AcceptanceTestBase : IClassFixture<CustomWebApplicationFactory>, IAsyncLifetime
    {
        private readonly IServiceScope _scope;
        protected readonly HttpClient _client;
        protected readonly RunningPlannerDbContext _dbContext;

        public AcceptanceTestBase(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
            _scope = factory.Services.CreateScope();
            _dbContext = _scope.ServiceProvider.GetRequiredService<RunningPlannerDbContext>();
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            await _dbContext.Database.EnsureDeletedAsync();
            _scope.Dispose();
        }
    }
}