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
            var route = new RunningRoute
            {
                Name = "Test Route",
                Geometry = new GeoJsonLineString
                {
                    Coordinates = new List<List<double>>
                    {
                        new() { 12.5683, 55.6761 },
                        new() { 12.5685, 55.6765 }
                    }
                }
            };

            await _repository.AddAsync(route);
            var result = await _repository.GetAllAsync();

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test Route", result.First().Name);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllRoutes()
        {
            var route1 = new RunningRoute
            {
                Name = "Route 1",
                Geometry = new GeoJsonLineString
                {
                    Coordinates = new List<List<double>>
                    {
                        new() { 12.5683, 55.6761 },
                        new() { 12.5685, 55.6765 }
                    }
                }
            };
            var route2 = new RunningRoute
            {
                Name = "Route 2",
                Geometry = new GeoJsonLineString
                {
                    Coordinates = new List<List<double>>
                    {
                        new() { 12.5683, 55.6761 },
                        new() { 12.5685, 55.6765 }
                    }
                }
            };

            await _repository.AddAsync(route1);
            await _repository.AddAsync(route2);

            var result = await _repository.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnCorrectRoute()
        {
            var route = new RunningRoute
            {
                Name = "Special Route",
                Geometry = new GeoJsonLineString
                {
                    Coordinates = new List<List<double>>
                    {
                        new() { 12.5683, 55.6761 },
                        new() { 12.5685, 55.6765 }
                    }
                }
            };
            await _repository.AddAsync(route);

            var inserted = (await _repository.GetAllAsync())!.First();
            var result = await _repository.GetByIdAsync(inserted.ID);

            Assert.NotNull(result);
            Assert.Equal("Special Route", result!.Name);
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyRoute()
        {
            var route = new RunningRoute
            {
                Name = "Old Name",
                Geometry = new GeoJsonLineString
                {
                    Coordinates = new List<List<double>>
                    {
                        new() { 12.5683, 55.6761 },
                        new() { 12.5685, 55.6765 }
                    }
                }
            };
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
            var route = new RunningRoute
            {
                Name = "To Be Deleted",
                Geometry = new GeoJsonLineString
                {
                    Coordinates = new List<List<double>>
                    {
                        new() { 12.5683, 55.6761 },
                        new() { 12.5685, 55.6765 }
                    }
                }
            };
            await _repository.AddAsync(route);

            var inserted = (await _repository.GetAllAsync())!.First();
            await _repository.DeleteAsync(inserted.ID);

            var result = await _repository.GetAllAsync();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task FindRoutesNearPointAsync_ShouldReturnNearbyRoutes_LineString()
        {
            var route = new RunningRoute
            {
                Name = "Nearby Line Route",
                Geometry = new GeoJsonLineString
                {
                    Coordinates = new List<List<double>>
            {
                new() { 12.5, 55.7 },
                new() { 12.6, 55.8 }
            }
                }
            };

            await _repository.AddAsync(route);

            var results = await _repository.FindRoutesNearPointAsync(12.55, 55.75, 20000);

            Assert.NotNull(results);
            Assert.Single(results);
            Assert.Equal("Nearby Line Route", results[0].Name);
        }

        [Fact]
        public async Task GetRoutesWithinPolygonAsync_ShouldReturnLineRoutesInsidePolygon()
        {
            var route = new RunningRoute
            {
                UserID = 0,
                Name = "Within Route",
                Geometry = new GeoJsonLineString
                {
                    Coordinates = new List<List<double>>
                    {
                        new() { 12.568337, 55.676098 },
                        new() { 12.568337, 55.677000 }
                    }
                },
                CreatedAt = DateTime.UtcNow,
                DistanceKm = 1.2
            };

            await _repository.AddAsync(route);


            var polygon = new List<List<double>>
            {
                new() { 12.4, 55.6 },
                new() { 12.6, 55.6 },
                new() { 12.6, 55.8 },
                new() { 12.4, 55.8 },
                new() { 12.4, 55.6 }
            };

            var results = await _repository.GetRoutesWithinPolygonAsync(polygon);

            Assert.NotNull(results);
            Assert.Single(results);
            Assert.Equal("Within Route", results[0].Name);
        }

        [Fact]
        public async Task GetRoutesGeoIntersectsAsync_ShouldReturnIntersectingLineRoutes()
        {
            var route = new RunningRoute
            {
                UserID = 0,
                Name = "Intersecting Route",
                Geometry = new GeoJsonLineString
                {
                    Coordinates = new List<List<double>>
                    {
                        new() { 12.568337, 55.676098 },
                        new() { 12.568337, 55.677000 }
                    }
                },
                CreatedAt = DateTime.UtcNow,
                DistanceKm = 1.2
            };

            await _repository.AddAsync(route);


            var polygon = new List<List<double>>
            {
                new() { 12.4, 55.6 },
                new() { 12.6, 55.6 },
                new() { 12.6, 55.8 },
                new() { 12.4, 55.8 },
                new() { 12.4, 55.6 }
            };

            var results = await _repository.GetRoutesGeoIntersectsAsync(polygon);

            Assert.NotNull(results);
            Assert.Single(results);
            Assert.Equal("Intersecting Route", results[0].Name);
        }

        public void Dispose()
        {
            _runner.Dispose();
        }
    }
}
