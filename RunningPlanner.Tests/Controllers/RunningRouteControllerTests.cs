using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using RunningPlanner.Controllers;
using RunningPlanner.Models;
using RunningPlanner.Services;

namespace RunningPlanner.Tests
{
    public class RunningRouteControllerTests
    {
        private readonly Mock<IRunningRouteService> _serviceMock;
        private readonly RunningRouteController _controller;

        public RunningRouteControllerTests()
        {
            _serviceMock = new Mock<IRunningRouteService>();
            _controller = new RunningRouteController(_serviceMock.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnListOfRoutes()
        {
            var routes = new List<RunningRoute> { new RunningRoute(), new RunningRoute() };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(routes);

            var result = await _controller.GetAll();

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<List<RunningRoute>>(okResult.Value);
            Assert.Equal(2, value.Count);
        }

        [Fact]
        public async Task GetById_ShouldReturnRoute_IfExists()
        {
            var route = new RunningRoute { ID = "route1" };
            _serviceMock.Setup(s => s.GetByIdAsync("route1")).ReturnsAsync(route);

            var result = await _controller.GetById("route1");

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<RunningRoute>(okResult.Value);
            Assert.Equal("route1", value.ID);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_IfMissing()
        {
            _serviceMock.Setup(s => s.GetByIdAsync("missing")).ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.GetById("missing");

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task Add_ShouldReturnCreated_IfRouteIsValid()
        {
            var route = new RunningRoute
            {
                ID = "newroute",
                Name = "Test Route",
                CreatedAt = DateTime.UtcNow,
                DistanceKm = 5.0
            };

            var userId = 123;

            var claims = new List<Claim> { new Claim("UserID", userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var result = await _controller.Add(route);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var value = Assert.IsType<RunningRoute>(createdResult.Value);

            Assert.Equal("newroute", value.ID);
            Assert.Equal(userId, value.UserID);
        }

        [Fact]
        public async Task Add_ShouldReturnBadRequest_IfRouteIsNull()
        {
            var result = await _controller.Add(null!);

            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Route data is required.", badResult.Value);
        }

        [Fact]
        public async Task Update_ShouldReturnCreated_IfUpdateSucceeds()
        {
            var route = new RunningRoute
            {
                ID = "update1",
                Name = "Updated Route",
                CreatedAt = DateTime.UtcNow,
                DistanceKm = 8.0
            };

            var userId = 456;

            var claims = new List<Claim> { new Claim("UserID", userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            var result = await _controller.Update(route);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var value = Assert.IsType<RunningRoute>(createdResult.Value);

            Assert.Equal("update1", value.ID);
            Assert.Equal(userId, value.UserID);
        }

        [Fact]
        public async Task Update_ShouldReturnBadRequest_IfRouteIsNull()
        {
            var result = await _controller.Update(null!);

            var badResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Route data is required.", badResult.Value);
        }

        [Fact]
        public async Task Update_ShouldReturnNotFound_IfRouteNotFound()
        {
            var route = new RunningRoute { ID = "missing" };

            var userId = 789;
            var claims = new List<Claim> { new Claim("UserID", userId.ToString()) };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = claimsPrincipal }
            };

            _serviceMock.Setup(s => s.UpdateAsync("missing", It.IsAny<RunningRoute>()))
                .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.Update(route);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Delete_ShouldReturnOk_IfRouteExists()
        {
            var result = await _controller.Delete("route1");

            var okResult = Assert.IsType<OkObjectResult>(result);
            var message = Assert.IsType<string>(okResult.Value);
            Assert.Contains("route1", message);
        }

        [Fact]
        public async Task Delete_ShouldReturnNotFound_IfRouteMissing()
        {
            _serviceMock.Setup(s => s.DeleteAsync("missing")).ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.Delete("missing");

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetRoutesNearPoint_ReturnsOk_WhenPointIsValid()
        {
            var point = new GeoPoint { Longitude = 12.5, Latitude = 55.6, MaxDistanceMeters = 500 };
            var expectedRoutes = new List<RunningRoute> { new() { Name = "Nearby Route" } };

            _serviceMock.Setup(s => s.GetRoutesNearPointAsync(point.Longitude, point.Latitude, point.MaxDistanceMeters))
                        .ReturnsAsync(expectedRoutes);

            var result = await _controller.GetRoutesNearPoint(point);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedRoutes, ok.Value);
        }

        [Fact]
        public async Task GetRoutesNearPoint_ReturnsBadRequest_WhenPointIsNull()
        {
            var result = await _controller.GetRoutesNearPoint(null!);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Point data is required.", badRequest.Value);
        }

        [Fact]
        public async Task GetRoutesWithinPolygon_ReturnsOk_WhenPolygonIsValid()
        {
            var polygon = new Polygon
            {
                Coordinates = new List<List<double>> {
                    new() { 0, 0 },
                    new() { 0, 1 },
                    new() { 1, 0 }
                }
            };

            var expectedRoutes = new List<RunningRoute> { new() { Name = "Inside Polygon" } };

            _serviceMock.Setup(s => s.GetRoutesWithinPolygonAsync(polygon))
                        .ReturnsAsync(expectedRoutes);

            var result = await _controller.GetRoutesWithinPolygon(polygon);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedRoutes, ok.Value);
        }

        [Fact]
        public async Task GetRoutesWithinPolygon_ReturnsBadRequest_WhenPolygonIsNull()
        {
            var result = await _controller.GetRoutesWithinPolygon(null!);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("A valid polygon with at least 3 coordinates is required.", badRequest.Value);
        }

        [Fact]
        public async Task GetRoutesWithinPolygon_ReturnsBadRequest_WhenTooFewCoordinates()
        {
            var polygon = new Polygon
            {
                Coordinates = new List<List<double>> {
                    new() { 0, 0 },
                    new() { 1, 1 }
                }
            };

            var result = await _controller.GetRoutesWithinPolygon(polygon);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("A valid polygon with at least 3 coordinates is required.", badRequest.Value);
        }

        [Fact]
        public async Task GetRoutesIntersectingPolygon_ReturnsOk_WhenValidPolygon()
        {
            var polygon = new Polygon
            {
                Coordinates = new List<List<double>> {
                    new() { 0, 0 },
                    new() { 0, 1 },
                    new() { 1, 1 },
                    new() { 0, 0 }
                }
            };

            var expectedRoutes = new List<RunningRoute> { new() { Name = "Crossing Route" } };

            _serviceMock.Setup(s => s.GetRoutesIntersectingPolygonAsync(polygon.Coordinates))
                        .ReturnsAsync(expectedRoutes);

            var result = await _controller.GetRoutesIntersectingPolygon(polygon);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(expectedRoutes, ok.Value);
        }

        [Fact]
        public async Task GetRoutesIntersectingPolygon_ReturnsBadRequest_WhenPolygonIsNull()
        {
            var result = await _controller.GetRoutesIntersectingPolygon(null!);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Polygon coordinates must have at least 4 points (including closing point).", badRequest.Value);
        }

        [Fact]
        public async Task GetRoutesIntersectingPolygon_ReturnsBadRequest_WhenCoordinatesAreNull()
        {
            var polygon = new Polygon { Coordinates = null! };

            var result = await _controller.GetRoutesIntersectingPolygon(polygon);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Polygon coordinates must have at least 4 points (including closing point).", badRequest.Value);
        }

        [Fact]
        public async Task GetRoutesIntersectingPolygon_ReturnsBadRequest_WhenTooFewPoints()
        {
            var polygon = new Polygon
            {
                Coordinates = new List<List<double>> {
                    new() { 0, 0 },
                    new() { 1, 1 },
                    new() { 1, 0 }
                }
            };

            var result = await _controller.GetRoutesIntersectingPolygon(polygon);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Polygon coordinates must have at least 4 points (including closing point).", badRequest.Value);
        }
    }
}
