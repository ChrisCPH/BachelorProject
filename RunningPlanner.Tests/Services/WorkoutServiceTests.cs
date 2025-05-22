using Moq;
using RunningPlanner.Models;
using RunningPlanner.Repositories;
using RunningPlanner.Services;

namespace RunningPlanner.Tests.Services
{
    public class WorkoutServiceTests
    {
        private readonly Mock<ITrainingPlanRepository> _trainingPlanRepositoryMock;
        private readonly Mock<IWorkoutRepository> _workoutRepositoryMock;
        private readonly WorkoutService _workoutService;

        public WorkoutServiceTests()
        {
            _trainingPlanRepositoryMock = new Mock<ITrainingPlanRepository>();
            _workoutRepositoryMock = new Mock<IWorkoutRepository>();
            _workoutService = new WorkoutService(_workoutRepositoryMock.Object, _trainingPlanRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateWorkoutAsync_ShouldReturnCreatedWorkout_WhenWorkoutIsValid()
        {
            var workout = new Workout { WorkoutID = 1, DayOfWeek = (DayOfWeek)1, WeekNumber = 1 };
            _workoutRepositoryMock.Setup(repo => repo.AddWorkoutAsync(workout)).ReturnsAsync(workout);

            var result = await _workoutService.CreateWorkoutAsync(workout);

            Assert.Equal(workout, result);
            _workoutRepositoryMock.Verify(repo => repo.AddWorkoutAsync(workout), Times.Once);
        }

        [Fact]
        public async Task CreateWorkoutAsync_ShouldThrowArgumentNullException_WhenWorkoutIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _workoutService.CreateWorkoutAsync(null!));
        }

        [Fact]
        public async Task GetWorkoutByIdAsync_ShouldReturnWorkout_WhenWorkoutExists()
        {
            var workout = new Workout { WorkoutID = 1 };
            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutByIdAsync(workout.WorkoutID)).ReturnsAsync(workout);

            var result = await _workoutService.GetWorkoutByIdAsync(workout.WorkoutID);

            Assert.Equal(workout, result);
            _workoutRepositoryMock.Verify(repo => repo.GetWorkoutByIdAsync(workout.WorkoutID), Times.Once);
        }

        [Fact]
        public async Task GetAllWorkoutsByTrainingPlanAsync_ShouldReturnWorkouts_WhenWorkoutsExist()
        {
            int runId = 1;
            var workouts = new List<Workout> { new Workout { WorkoutID = 1 }, new Workout { WorkoutID = 2 } };
            _workoutRepositoryMock.Setup(repo => repo.GetAllWorkoutsByTrainingPlanAsync(runId)).ReturnsAsync(workouts);

            var result = await _workoutService.GetAllWorkoutsByTrainingPlanAsync(runId);

            Assert.Equal(workouts, result);
            _workoutRepositoryMock.Verify(repo => repo.GetAllWorkoutsByTrainingPlanAsync(runId), Times.Once);
        }

        [Fact]
        public async Task UpdateWorkoutAsync_ShouldReturnUpdatedWorkout_WhenWorkoutIsValid()
        {
            var workout = new Workout { WorkoutID = 1 };
            _workoutRepositoryMock.Setup(repo => repo.UpdateWorkoutAsync(workout)).ReturnsAsync(workout);

            var result = await _workoutService.UpdateWorkoutAsync(workout);

            Assert.Equal(workout, result);
            _workoutRepositoryMock.Verify(repo => repo.UpdateWorkoutAsync(workout), Times.Once);
        }

        [Fact]
        public async Task UpdateWorkoutAsync_ShouldThrowArgumentNullException_WhenWorkoutIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _workoutService.UpdateWorkoutAsync(null!));
        }

        [Fact]
        public async Task DeleteWorkoutAsync_ShouldReturnTrue_WhenWorkoutIsDeleted()
        {
            int workoutId = 1;
            _workoutRepositoryMock.Setup(repo => repo.DeleteWorkoutAsync(workoutId)).ReturnsAsync(true);

            var result = await _workoutService.DeleteWorkoutAsync(workoutId);

            Assert.True(result);
            _workoutRepositoryMock.Verify(repo => repo.DeleteWorkoutAsync(workoutId), Times.Once);
        }
    }
}
