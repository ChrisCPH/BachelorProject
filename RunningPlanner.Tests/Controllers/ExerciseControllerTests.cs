using Microsoft.AspNetCore.Mvc;
using Moq;
using RunningPlanner.Controllers;
using RunningPlanner.Models;
using RunningPlanner.Services;

namespace RunningPlanner.Tests.Controllers
{
    public class ExerciseControllerTests
    {
        private readonly Mock<IExerciseService> _exerciseServiceMock;
        private readonly ExerciseController _controller;

        public ExerciseControllerTests()
        {
            _exerciseServiceMock = new Mock<IExerciseService>();
            _controller = new ExerciseController(_exerciseServiceMock.Object);
        }

        [Fact]
        public async Task CreateExercise_ReturnsCreatedAtAction_WhenExerciseIsValid()
        {
            var exercise = new Exercise { ExerciseID = 1 };
            _exerciseServiceMock.Setup(s => s.CreateExerciseAsync(exercise)).ReturnsAsync(exercise);

            var result = await _controller.CreateExercise(exercise);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(ExerciseController.GetExerciseById), createdResult.ActionName);
            Assert.Equal(exercise, createdResult.Value);
        }

        [Fact]
        public async Task CreateExercise_ReturnsBadRequest_WhenExerciseIsNull()
        {
            var result = await _controller.CreateExercise(null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetExerciseById_ReturnsOk_WhenExerciseExists()
        {
            var exercise = new Exercise { ExerciseID = 1 };
            _exerciseServiceMock.Setup(s => s.GetExerciseByIdAsync(1)).ReturnsAsync(exercise);

            var result = await _controller.GetExerciseById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(exercise, okResult.Value);
        }

        [Fact]
        public async Task GetExerciseById_ReturnsNotFound_WhenExerciseDoesNotExist()
        {
            _exerciseServiceMock.Setup(s => s.GetExerciseByIdAsync(1)).ReturnsAsync((Exercise)null!);

            var result = await _controller.GetExerciseById(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Exercise not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetAllExercisesByWorkout_ReturnsOk_WhenExercisesExist()
        {
            var exercises = new List<Exercise> { new Exercise { ExerciseID = 1 } };
            _exerciseServiceMock.Setup(s => s.GetAllExercisesByWorkoutAsync(1)).ReturnsAsync(exercises);

            var result = await _controller.GetAllExercisesByWorkout(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(exercises, okResult.Value);
        }

        [Fact]
        public async Task GetAllExercisesByWorkout_ReturnsNotFound_WhenNoExercisesFound()
        {
            _exerciseServiceMock.Setup(s => s.GetAllExercisesByWorkoutAsync(1)).ReturnsAsync(new List<Exercise>());

            var result = await _controller.GetAllExercisesByWorkout(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No exercises found for the specified workout.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateExercise_ReturnsOk_WhenExerciseIsUpdated()
        {
            var exercise = new Exercise { ExerciseID = 1 };
            _exerciseServiceMock.Setup(s => s.UpdateExerciseAsync(exercise)).ReturnsAsync(exercise);

            var result = await _controller.UpdateExercise(exercise);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(exercise, okResult.Value);
        }

        [Fact]
        public async Task UpdateExercise_ReturnsBadRequest_WhenExerciseIsNull()
        {
            var result = await _controller.UpdateExercise(null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task DeleteExercise_ReturnsNoContent_WhenExerciseIsDeleted()
        {
            _exerciseServiceMock.Setup(s => s.DeleteExerciseAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteExercise(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteExercise_ReturnsNotFound_WhenExerciseDoesNotExist()
        {
            _exerciseServiceMock.Setup(s => s.DeleteExerciseAsync(1)).ReturnsAsync(false);

            var result = await _controller.DeleteExercise(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Exercise not found.", notFoundResult.Value);
        }
    }
}
