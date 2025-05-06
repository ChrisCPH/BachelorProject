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
            var route = new RunningRoute { ID = "newroute" };

            var result = await _controller.Add(route);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var value = Assert.IsType<RunningRoute>(createdResult.Value);
            Assert.Equal("newroute", value.ID);
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
            var route = new RunningRoute { ID = "update1" };

            var result = await _controller.Update(route);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            var value = Assert.IsType<RunningRoute>(createdResult.Value);
            Assert.Equal("update1", value.ID);
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
            _serviceMock.Setup(s => s.UpdateAsync("missing", route)).ThrowsAsync(new KeyNotFoundException());

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
    }
}
