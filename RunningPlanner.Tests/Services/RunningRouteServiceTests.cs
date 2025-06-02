using Moq;
using RunningPlanner.Models;
using RunningPlanner.Repositories;
using RunningPlanner.Services;
using Xunit;

namespace RunningPlanner.Tests
{
    public class RunningRouteServiceTests
    {
        private readonly Mock<IRunningRouteRepository> _routeRepoMock;
        private readonly Mock<IRunRepository> _runRepoMock;
        private readonly RunningRouteService _service;

        public RunningRouteServiceTests()
        {
            _routeRepoMock = new Mock<IRunningRouteRepository>();
            _runRepoMock = new Mock<IRunRepository>();
            _service = new RunningRouteService(_routeRepoMock.Object, _runRepoMock.Object);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllRoutes()
        {
            var routes = new List<RunningRoute>
            {
                new RunningRoute { Name = "Route 1" },
                new RunningRoute { Name = "Route 2" }
            };

            _routeRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(routes);

            var result = await _service.GetAllAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("Route 1", result[0].Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnRoute_WhenExists()
        {
            var route = new RunningRoute { Name = "My Route" };

            _routeRepoMock.Setup(r => r.GetByIdAsync("123")).ReturnsAsync(route);

            var result = await _service.GetByIdAsync("123");

            Assert.NotNull(result);
            Assert.Equal("My Route", result.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrow_WhenNotFound()
        {
            _routeRepoMock.Setup(r => r.GetByIdAsync("missing")).ReturnsAsync((RunningRoute?)null);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.GetByIdAsync("missing"));

            Assert.Equal("Route with id missing not found", ex.Message);
        }

        [Fact]
        public async Task AddAsync_ShouldCallRepository()
        {
            var route = new RunningRoute { Name = "New Route" };

            await _service.AddAsync(route);

            _routeRepoMock.Verify(r => r.AddAsync(route), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenNotFound()
        {
            var route = new RunningRoute { Name = "Update Route" };
            _routeRepoMock.Setup(r => r.GetByIdAsync("bad-id")).ReturnsAsync((RunningRoute?)null);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.UpdateAsync("bad-id", route));

            Assert.Equal("Route with id bad-id not found", ex.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldCallRepository_WhenFound()
        {
            var route = new RunningRoute { Name = "Old" };

            _routeRepoMock.Setup(r => r.GetByIdAsync("route1")).ReturnsAsync(route);

            await _service.UpdateAsync("route1", route);

            _routeRepoMock.Verify(r => r.UpdateAsync("route1", route), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrow_WhenNotFound()
        {
            _routeRepoMock.Setup(r => r.GetByIdAsync("nope")).ReturnsAsync((RunningRoute?)null);

            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.DeleteAsync("nope"));

            Assert.Equal("Route with id nope not found", ex.Message);

            _runRepoMock.Verify(r => r.RemoveRouteIdFromRunsAsync(It.IsAny<string>()), Times.Never);
            _routeRepoMock.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldCallRemoveRouteIdAndDelete_WhenFound()
        {
            var route = new RunningRoute { Name = "ToDelete" };
            _routeRepoMock.Setup(r => r.GetByIdAsync("id1")).ReturnsAsync(route);

            await _service.DeleteAsync("id1");

            _runRepoMock.Verify(r => r.RemoveRouteIdFromRunsAsync("id1"), Times.Once);
            _routeRepoMock.Verify(r => r.DeleteAsync("id1"), Times.Once);
        }

        [Fact]
        public async Task GetRoutesNearPointAsync_CallsRepositoryAndReturnsData()
        {
            var expectedRoutes = new List<RunningRoute>
            {
                new() { Name = "Nearby Route" }
            };

            _routeRepoMock.Setup(r => r.FindRoutesNearPointAsync(12.5, 55.6, 100))
                     .ReturnsAsync(expectedRoutes);

            var result = await _service.GetRoutesNearPointAsync(12.5, 55.6, 100);

            Assert.Equal(expectedRoutes, result);
            _routeRepoMock.Verify(r => r.FindRoutesNearPointAsync(12.5, 55.6, 100), Times.Once);
        }

        [Fact]
        public async Task GetRoutesWithinPolygonAsync_CallsRepositoryAndReturnsData()
        {
            var polygon = new Polygon
            {
                Coordinates = new List<List<double>>
                {
                    new() { 12.4, 55.6 },
                    new() { 12.6, 55.6 },
                    new() { 12.6, 55.8 },
                    new() { 12.4, 55.8 },
                    new() { 12.4, 55.6 }
                }
            };

            var expectedRoutes = new List<RunningRoute>
            {
                new() { Name = "Inside Polygon" }
            };

            _routeRepoMock.Setup(r => r.GetRoutesWithinPolygonAsync(polygon.Coordinates))
                     .ReturnsAsync(expectedRoutes);

            var result = await _service.GetRoutesWithinPolygonAsync(polygon);

            Assert.Equal(expectedRoutes, result);
            _routeRepoMock.Verify(r => r.GetRoutesWithinPolygonAsync(polygon.Coordinates), Times.Once);
        }

        [Fact]
        public async Task GetRoutesIntersectingPolygonAsync_CallsRepositoryAndReturnsData()
        {
            var polygonCoordinates = new List<List<double>>
            {
                new() { 12.4, 55.6 },
                new() { 12.6, 55.6 },
                new() { 12.6, 55.8 },
                new() { 12.4, 55.8 },
                new() { 12.4, 55.6 }
            };

            var expectedRoutes = new List<RunningRoute>
            {
                new() { Name = "Intersecting Route" }
            };

            _routeRepoMock.Setup(r => r.GetRoutesGeoIntersectsAsync(polygonCoordinates))
                     .ReturnsAsync(expectedRoutes);

            var result = await _service.GetRoutesIntersectingPolygonAsync(polygonCoordinates);

            Assert.Equal(expectedRoutes, result);
            _routeRepoMock.Verify(r => r.GetRoutesGeoIntersectsAsync(polygonCoordinates), Times.Once);
        }
    }
}
