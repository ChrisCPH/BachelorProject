using Microsoft.AspNetCore.Mvc;
using Moq;
using RunningPlanner.Controllers;
using RunningPlanner.Models;
using RunningPlanner.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace RunningPlanner.Tests.Controllers
{
    public class RunControllerTests
    {
        private readonly Mock<IRunService> _runServiceMock;
        private readonly RunController _runController;

        public RunControllerTests()
        {
            _runServiceMock = new Mock<IRunService>();
            _runController = new RunController(_runServiceMock.Object);
        }

        [Fact]
        public async Task CreateRun_ShouldReturnCreatedAtAction_WhenRunIsValid()
        {
            var run = new Run { RunID = 1 };
            _runServiceMock.Setup(s => s.CreateRunAsync(run)).ReturnsAsync(run);

            var result = await _runController.CreateRun(run);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_runController.GetRunById), createdAtActionResult.ActionName);
            Assert.Equal(run, createdAtActionResult.Value);
        }

        [Fact]
        public async Task CreateRun_ShouldReturnBadRequest_WhenRunIsNull()
        {
            var result = await _runController.CreateRun(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Run data is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetRunById_ShouldReturnOk_WhenRunExists()
        {
            var run = new Run { RunID = 1 };
            _runServiceMock.Setup(s => s.GetRunByIdAsync(run.RunID)).ReturnsAsync(run);

            var result = await _runController.GetRunById(run.RunID);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(run, okResult.Value);
        }

        [Fact]
        public async Task GetRunById_ShouldReturnNotFound_WhenRunDoesNotExist()
        {
            _runServiceMock.Setup(s => s.GetRunByIdAsync(It.IsAny<int>())).ReturnsAsync((Run)null!);

            var result = await _runController.GetRunById(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAllRunsByTrainingPlan_ShouldReturnOk_WhenRunsExist()
        {
            var runs = new List<Run> { new Run { RunID = 1 }, new Run { RunID = 2 } };
            _runServiceMock.Setup(s => s.GetAllRunsByTrainingPlanAsync(It.IsAny<int>())).ReturnsAsync(runs);

            var result = await _runController.GetAllRunsByTrainingPlan(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(runs, okResult.Value);
        }

        [Fact]
        public async Task GetAllRunsByTrainingPlan_ShouldReturnNotFound_WhenNoRunsExist()
        {
            _runServiceMock.Setup(s => s.GetAllRunsByTrainingPlanAsync(It.IsAny<int>())).ReturnsAsync(new List<Run>());

            var result = await _runController.GetAllRunsByTrainingPlan(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No runs found for the specified training plan.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateRun_ShouldReturnOk_WhenRunIsUpdated()
        {
            var run = new Run { RunID = 1 };
            _runServiceMock.Setup(s => s.UpdateRunAsync(run)).ReturnsAsync(run);

            var result = await _runController.UpdateRun(run);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(run, okResult.Value);
        }

        [Fact]
        public async Task UpdateRun_ShouldReturnBadRequest_WhenRunIsNull()
        {
            var result = await _runController.UpdateRun(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Run data is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task DeleteRun_ShouldReturnNoContent_WhenRunIsDeleted()
        {
            _runServiceMock.Setup(s => s.DeleteRunAsync(It.IsAny<int>())).ReturnsAsync(true);

            var result = await _runController.DeleteRun(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteRun_ShouldReturnNotFound_WhenRunDoesNotExist()
        {
            _runServiceMock.Setup(s => s.DeleteRunAsync(It.IsAny<int>())).ReturnsAsync(false);

            var result = await _runController.DeleteRun(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Run not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateRunCompletedStatus_ReturnsOk_WhenRunIsUpdated()
        {
            var run = new Run { RunID = 1, Completed = false };
            var updatedRun = new Run { RunID = 1, Completed = true };

            _runServiceMock.Setup(s => s.GetRunByIdAsync(run.RunID))
                .ReturnsAsync(run);
            _runServiceMock.Setup(s => s.UpdateRunAsync(run))
                .ReturnsAsync(updatedRun);

            var result = await _runController.UpdateRunCompletedStatus(run.RunID, true);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updatedRun, okResult.Value);
        }

        [Fact]
        public async Task UpdateRunCompletedStatus_ReturnsNotFound_WhenRunDoesNotExist()
        {
            var runId = 1;

            _runServiceMock.Setup(s => s.GetRunByIdAsync(runId))
                .ReturnsAsync((Run)null!);

            var result = await _runController.UpdateRunCompletedStatus(runId, true);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Run not found.", notFoundResult.Value);
        }
    }
}
