using Mongo2Go;
using MongoDB.Driver;
using RunningPlanner.Models;
using RunningPlanner.Repositories;
using Xunit;

namespace RunningPlanner.Tests
{
    public class RunningRouteRepositoryTests : IDisposable
    {
        private readonly MongoDbRunner _runner;
        private readonly IMongoDatabase _database;
        private readonly RunningRouteRepository _repository;

        public RunningRouteRepositoryTests()
        {
            _runner = MongoDbRunner.Start();
            var client = new MongoClient(_runner.ConnectionString);
            _database = client.GetDatabase("RunningPlannerTestDb");
            _repository = new RunningRouteRepository(_database);
        }

        [Fact]
        public async Task AddAsync_ShouldInsertRoute()
        {
            var route = new RunningRoute { Name = "Test Route" };

            await _repository.AddAsync(route);
            var result = await _repository.GetAllAsync();

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Route", result.First().Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllRoutes()
        {
            var route1 = new RunningRoute { Name = "Route 1" };
            var route2 = new RunningRoute { Name = "Route 2" };

            await _repository.AddAsync(route1);
            await _repository.AddAsync(route2);

            var result = await _repository.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectRoute()
        {
            var route = new RunningRoute { Name = "Special Route" };
            await _repository.AddAsync(route);

            var inserted = (await _repository.GetAllAsync())!.First();
            var result = await _repository.GetByIdAsync(inserted.ID);

            Assert.NotNull(result);
            Assert.Equal("Special Route", result!.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyRoute()
        {
            var route = new RunningRoute { Name = "Old Name" };
            await _repository.AddAsync(route);

            var inserted = (await _repository.GetAllAsync())!.First();
            inserted.Name = "New Name";

            await _repository.UpdateAsync(inserted.ID, inserted);

            var updated = await _repository.GetByIdAsync(inserted.ID);
            Assert.Equal("New Name", updated!.Name);
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveRoute()
        {
            var route = new RunningRoute { Name = "To Be Deleted" };
            await _repository.AddAsync(route);

            var inserted = (await _repository.GetAllAsync())!.First();
            await _repository.DeleteAsync(inserted.ID);

            var result = await _repository.GetAllAsync();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        public void Dispose()
        {
            _runner.Dispose();
        }
    }
}
