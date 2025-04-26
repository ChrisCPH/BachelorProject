using Microsoft.AspNetCore.Mvc;
using Moq;
using RunningPlanner.Controllers;
using RunningPlanner.Models;
using RunningPlanner.Services;

namespace RunningPlanner.Tests.Controllers
{
    public class TrainingPlanControllerTests
    {
        private readonly Mock<ITrainingPlanService> _trainingPlanServiceMock;
        private readonly TrainingPlanController _trainingPlanController;

        public TrainingPlanControllerTests()
        {
            _trainingPlanServiceMock = new Mock<ITrainingPlanService>();
            _trainingPlanController = new TrainingPlanController(_trainingPlanServiceMock.Object);
        }

        [Fact]
        public async Task CreateTrainingPlan_ShouldReturnCreatedAtAction_WhenTrainingPlanIsValid()
        {
            var trainingPlan = new TrainingPlan { TrainingPlanID = 1 };
            int userId = 1;
            _trainingPlanServiceMock.Setup(s => s.CreateTrainingPlanAsync(trainingPlan, userId)).ReturnsAsync(trainingPlan);

            var result = await _trainingPlanController.CreateTrainingPlan(userId, trainingPlan);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_trainingPlanController.GetTrainingPlanById), createdAtActionResult.ActionName);
            Assert.Equal(trainingPlan, createdAtActionResult.Value);
        }

        [Fact]
        public async Task CreateTrainingPlan_ShouldReturnBadRequest_WhenTrainingPlanIsNull()
        {
            var result = await _trainingPlanController.CreateTrainingPlan(1, null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Training plan data is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetTrainingPlanById_ShouldReturnOk_WhenTrainingPlanExists()
        {
            var trainingPlan = new TrainingPlan { TrainingPlanID = 1 };
            _trainingPlanServiceMock.Setup(s => s.GetTrainingPlanByIdAsync(trainingPlan.TrainingPlanID)).ReturnsAsync(trainingPlan);

            var result = await _trainingPlanController.GetTrainingPlanById(trainingPlan.TrainingPlanID);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(trainingPlan, okResult.Value);
        }

        [Fact]
        public async Task GetTrainingPlanById_ShouldReturnNotFound_WhenTrainingPlanDoesNotExist()
        {
            _trainingPlanServiceMock.Setup(s => s.GetTrainingPlanByIdAsync(It.IsAny<int>())).ReturnsAsync((TrainingPlan?)null);

            var result = await _trainingPlanController.GetTrainingPlanById(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAllTrainingPlansByUser_ShouldReturnOk_WhenTrainingPlansExist()
        {
            var trainingPlans = new List<TrainingPlan> { new TrainingPlan { TrainingPlanID = 1 } };
            _trainingPlanServiceMock.Setup(s => s.GetAllTrainingPlansByUserAsync(1)).ReturnsAsync(trainingPlans);

            var result = await _trainingPlanController.GetAllTrainingPlansByUser(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(trainingPlans, okResult.Value);
        }

        [Fact]
        public async Task GetAllTrainingPlansByUser_ShouldReturnNotFound_WhenNoTrainingPlansFound()
        {
            _trainingPlanServiceMock.Setup(s => s.GetAllTrainingPlansByUserAsync(1)).ReturnsAsync(new List<TrainingPlan>());

            var result = await _trainingPlanController.GetAllTrainingPlansByUser(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No training plans found for the specified user.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateTrainingPlan_ShouldReturnOk_WhenTrainingPlanUpdated()
        {
            var trainingPlan = new TrainingPlan { TrainingPlanID = 1 };
            _trainingPlanServiceMock.Setup(s => s.UpdateTrainingPlanAsync(trainingPlan)).ReturnsAsync(trainingPlan);

            var result = await _trainingPlanController.UpdateTrainingPlan(trainingPlan);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(trainingPlan, okResult.Value);
        }

        [Fact]
        public async Task UpdateTrainingPlan_ShouldReturnBadRequest_WhenTrainingPlanIsNull()
        {
            var result = await _trainingPlanController.UpdateTrainingPlan(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Training plan data is required.", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateTrainingPlan_ShouldReturnNotFound_WhenTrainingPlanNotFound()
        {
            var trainingPlan = new TrainingPlan { TrainingPlanID = 1 };
            _trainingPlanServiceMock.Setup(s => s.UpdateTrainingPlanAsync(It.IsAny<TrainingPlan>())).ReturnsAsync((TrainingPlan)null!);

            var result = await _trainingPlanController.UpdateTrainingPlan(trainingPlan);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Training plan not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteTrainingPlan_ShouldReturnNoContent_WhenDeletionSuccessful()
        {
            _trainingPlanServiceMock.Setup(s => s.DeleteTrainingPlanAsync(1)).ReturnsAsync(true);

            var result = await _trainingPlanController.DeleteTrainingPlan(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteTrainingPlan_ShouldReturnNotFound_WhenTrainingPlanNotFound()
        {
            _trainingPlanServiceMock.Setup(s => s.DeleteTrainingPlanAsync(1)).ReturnsAsync(false);

            var result = await _trainingPlanController.DeleteTrainingPlan(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Training plan not found.", notFoundResult.Value);
        }
    }
}
