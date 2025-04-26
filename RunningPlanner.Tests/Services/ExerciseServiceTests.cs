using Moq;
using RunningPlanner.Models;
using RunningPlanner.Repositories;
using RunningPlanner.Services;

namespace RunningPlanner.Tests.Services
{
    public class ExerciseServiceTests
    {
        private readonly Mock<IExerciseRepository> _exerciseRepositoryMock;
        private readonly ExerciseService _exerciseService;

        public ExerciseServiceTests()
        {
            _exerciseRepositoryMock = new Mock<IExerciseRepository>();
            _exerciseService = new ExerciseService(_exerciseRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateExerciseAsync_ShouldReturnCreatedExercise_WhenExerciseIsValid()
        {
            var exercise = new Exercise { ExerciseID = 1 };
            _exerciseRepositoryMock.Setup(repo => repo.AddExerciseAsync(exercise)).ReturnsAsync(exercise);

            var result = await _exerciseService.CreateExerciseAsync(exercise);

            Assert.Equal(exercise, result);
            _exerciseRepositoryMock.Verify(repo => repo.AddExerciseAsync(exercise), Times.Once);
        }

        [Fact]
        public async Task CreateExerciseAsync_ShouldThrowArgumentNullException_WhenExerciseIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _exerciseService.CreateExerciseAsync(null!));
        }

        [Fact]
        public async Task GetExerciseByIdAsync_ShouldReturnExercise_WhenExerciseExists()
        {
            var exercise = new Exercise { ExerciseID = 1 };
            _exerciseRepositoryMock.Setup(repo => repo.GetExerciseByIdAsync(exercise.ExerciseID)).ReturnsAsync(exercise);

            var result = await _exerciseService.GetExerciseByIdAsync(exercise.ExerciseID);

            Assert.Equal(exercise, result);
            _exerciseRepositoryMock.Verify(repo => repo.GetExerciseByIdAsync(exercise.ExerciseID), Times.Once);
        }

        [Fact]
        public async Task GetAllExercisesByWorkoutAsync_ShouldReturnExercises_WhenExercisesExist()
        {
            int workoutId = 1;
            var exercises = new List<Exercise> { new Exercise { ExerciseID = 1, WorkoutID = workoutId } };
            _exerciseRepositoryMock.Setup(repo => repo.GetAllExercisesByWorkoutAsync(workoutId)).ReturnsAsync(exercises);

            var result = await _exerciseService.GetAllExercisesByWorkoutAsync(workoutId);

            Assert.Equal(exercises, result);
            _exerciseRepositoryMock.Verify(repo => repo.GetAllExercisesByWorkoutAsync(workoutId), Times.Once);
        }

        [Fact]
        public async Task UpdateExerciseAsync_ShouldReturnUpdatedExercise_WhenExerciseIsValid()
        {
            var exercise = new Exercise { ExerciseID = 1 };
            _exerciseRepositoryMock.Setup(repo => repo.UpdateExerciseAsync(exercise)).ReturnsAsync(exercise);

            var result = await _exerciseService.UpdateExerciseAsync(exercise);

            Assert.Equal(exercise, result);
            _exerciseRepositoryMock.Verify(repo => repo.UpdateExerciseAsync(exercise), Times.Once);
        }

        [Fact]
        public async Task UpdateExerciseAsync_ShouldThrowArgumentNullException_WhenExerciseIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _exerciseService.UpdateExerciseAsync(null!));
        }

        [Fact]
        public async Task DeleteExerciseAsync_ShouldReturnTrue_WhenExerciseIsDeleted()
        {
            int exerciseId = 1;
            _exerciseRepositoryMock.Setup(repo => repo.DeleteExerciseAsync(exerciseId)).ReturnsAsync(true);

            var result = await _exerciseService.DeleteExerciseAsync(exerciseId);

            Assert.True(result);
            _exerciseRepositoryMock.Verify(repo => repo.DeleteExerciseAsync(exerciseId), Times.Once);
        }
    }
}
