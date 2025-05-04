using Microsoft.AspNetCore.Mvc;
using Moq;
using RunningPlanner.Controllers;
using RunningPlanner.Models;
using RunningPlanner.Services;

namespace RunningPlanner.Tests.Controllers
{
    public class WorkoutControllerTests
    {
        private readonly Mock<IWorkoutService> _workoutServiceMock;
        private readonly WorkoutController _controller;

        public WorkoutControllerTests()
        {
            _workoutServiceMock = new Mock<IWorkoutService>();
            _controller = new WorkoutController(_workoutServiceMock.Object);
        }

        [Fact]
        public async Task CreateWorkout_ReturnsCreatedAtActionResult_WhenWorkoutIsValid()
        {
            var workout = new Workout { WorkoutID = 1 };
            _workoutServiceMock.Setup(s => s.CreateWorkoutAsync(workout)).ReturnsAsync(workout);

            var result = await _controller.CreateWorkout(workout);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(WorkoutController.GetWorkoutById), createdResult.ActionName);
            Assert.Equal(workout, createdResult.Value);
        }

        [Fact]
        public async Task CreateWorkout_ReturnsBadRequest_WhenWorkoutIsNull()
        {
            var result = await _controller.CreateWorkout(null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetWorkoutById_ReturnsOk_WhenWorkoutExists()
        {
            var workout = new Workout { WorkoutID = 1 };
            _workoutServiceMock.Setup(s => s.GetWorkoutByIdAsync(1)).ReturnsAsync(workout);

            var result = await _controller.GetWorkoutById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(workout, okResult.Value);
        }

        [Fact]
        public async Task GetWorkoutById_ReturnsNotFound_WhenWorkoutDoesNotExist()
        {
            _workoutServiceMock.Setup(s => s.GetWorkoutByIdAsync(1)).ReturnsAsync((Workout)null!);

            var result = await _controller.GetWorkoutById(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task GetAllWorkoutsByTrainingPlan_ReturnsOk_WhenWorkoutsExist()
        {
            var workouts = new List<Workout> { new Workout { WorkoutID = 1 } };
            _workoutServiceMock.Setup(s => s.GetAllWorkoutsByTrainingPlanAsync(1)).ReturnsAsync(workouts);

            var result = await _controller.GetAllWorkoutsByTrainingPlan(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(workouts, okResult.Value);
        }

        [Fact]
        public async Task GetAllWorkoutsByTrainingPlan_ReturnsNotFound_WhenNoWorkoutsExist()
        {
            _workoutServiceMock.Setup(s => s.GetAllWorkoutsByTrainingPlanAsync(1)).ReturnsAsync(new List<Workout>());

            var result = await _controller.GetAllWorkoutsByTrainingPlan(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No workouts found for the specified workout.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateWorkout_ReturnsOk_WhenWorkoutIsUpdated()
        {
            var workout = new Workout { WorkoutID = 1 };
            _workoutServiceMock.Setup(s => s.UpdateWorkoutAsync(workout)).ReturnsAsync(workout);

            var result = await _controller.UpdateWorkout(workout);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(workout, okResult.Value);
        }

        [Fact]
        public async Task UpdateWorkout_ReturnsBadRequest_WhenWorkoutIsNull()
        {
            var result = await _controller.UpdateWorkout(null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateWorkout_ReturnsNotFound_WhenWorkoutDoesNotExist()
        {
            var workout = new Workout { WorkoutID = 1 };
            _workoutServiceMock.Setup(s => s.UpdateWorkoutAsync(workout)).ReturnsAsync((Workout)null!);

            var result = await _controller.UpdateWorkout(workout);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Workout not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteWorkout_ReturnsNoContent_WhenWorkoutIsDeleted()
        {
            _workoutServiceMock.Setup(s => s.DeleteWorkoutAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteWorkout(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteWorkout_ReturnsNotFound_WhenWorkoutDoesNotExist()
        {
            _workoutServiceMock.Setup(s => s.DeleteWorkoutAsync(1)).ReturnsAsync(false);

            var result = await _controller.DeleteWorkout(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Workout not found.", notFoundResult.Value);
        }
    }
}
