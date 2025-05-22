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
            int workoutId = 1;
            var workouts = new List<Workout> { new Workout { WorkoutID = 1 }, new Workout { WorkoutID = 2 } };
            _workoutRepositoryMock.Setup(repo => repo.GetAllWorkoutsByTrainingPlanAsync(workoutId)).ReturnsAsync(workouts);

            var result = await _workoutService.GetAllWorkoutsByTrainingPlanAsync(workoutId);

            Assert.Equal(workouts, result);
            _workoutRepositoryMock.Verify(repo => repo.GetAllWorkoutsByTrainingPlanAsync(workoutId), Times.Once);
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

        [Fact]
        public async Task CreateRepeatedWorkoutAsync_ShouldCreateWorkoutsForEachWeek_WhenWorkoutAndTrainingPlanAreValid()
        {
            var trainingPlanId = 123;
            var trainingPlan = new TrainingPlan { TrainingPlanID = trainingPlanId, Duration = 4 };
            var inputWorkout = new Workout
            {
                TrainingPlanID = trainingPlanId,
                Type = "Strength",
                DayOfWeek = DayOfWeek.Monday,
                TimeOfDay = new TimeSpan(7, 0, 0),
                Duration = 3000,
                Notes = "Test notes",
                Completed = false
            };

            _trainingPlanRepositoryMock.Setup(repo => repo.GetTrainingPlanByIdAsync(trainingPlanId))
                                       .ReturnsAsync(trainingPlan);

            List<Workout>? workoutsAdded = null;
            _workoutRepositoryMock.Setup(repo => repo.AddWorkoutsAsync(It.IsAny<List<Workout>>()))
                              .Callback<List<Workout>>(workouts => workoutsAdded = workouts)
                              .ReturnsAsync((List<Workout> workouts) => workouts);

            var result = await _workoutService.CreateRepeatedWorkoutAsync(inputWorkout);

            Assert.NotNull(result);
            Assert.Equal(trainingPlan.Duration, result.Count);
            Assert.NotNull(workoutsAdded);
            Assert.Equal(trainingPlan.Duration, workoutsAdded!.Count);

            for (int week = 1; week <= trainingPlan.Duration; week++)
            {
                var workoutForWeek = workoutsAdded.FirstOrDefault(r => r.WeekNumber == week);
                Assert.NotNull(workoutForWeek);
                Assert.Equal(inputWorkout.TrainingPlanID, workoutForWeek.TrainingPlanID);
                Assert.Equal(inputWorkout.Type, workoutForWeek.Type);
                Assert.Equal(inputWorkout.DayOfWeek, workoutForWeek.DayOfWeek);
                Assert.Equal(inputWorkout.TimeOfDay, workoutForWeek.TimeOfDay);
                Assert.Equal(inputWorkout.Duration, workoutForWeek.Duration);
                Assert.Equal(inputWorkout.Notes, workoutForWeek.Notes);
                Assert.Equal(inputWorkout.Completed, workoutForWeek.Completed);
                Assert.True((DateTime.UtcNow - workoutForWeek.CreatedAt).TotalSeconds < 5);
            }

            _trainingPlanRepositoryMock.Verify(repo => repo.GetTrainingPlanByIdAsync(trainingPlanId), Times.Once);
            _workoutRepositoryMock.Verify(repo => repo.AddWorkoutsAsync(It.IsAny<List<Workout>>()), Times.Once);
        }

        [Fact]
        public async Task CreateRepeatedWorkoutAsync_ShouldThrowArgumentNullException_WhenWorkoutIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _workoutService.CreateRepeatedWorkoutAsync(null!));
        }

        [Fact]
        public async Task CreateRepeatedWorkoutAsync_ShouldThrowArgumentException_WhenTrainingPlanNotFound()
        {
            var workout = new Workout { TrainingPlanID = 999 };

            _trainingPlanRepositoryMock.Setup(repo => repo.GetTrainingPlanByIdAsync(workout.TrainingPlanID))
                                       .ReturnsAsync((TrainingPlan?)null);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _workoutService.CreateRepeatedWorkoutAsync(workout));
            Assert.Equal("Training plan not found", ex.Message);
        }

        [Fact]
        public async Task CreateRepeatedWorkoutAsync_ShouldReturnEmptyList_WhenTrainingPlanDurationIsZero()
        {
            var trainingPlanId = 123;
            var trainingPlan = new TrainingPlan { TrainingPlanID = trainingPlanId, Duration = 0 };
            var workout = new Workout { TrainingPlanID = trainingPlanId };

            _trainingPlanRepositoryMock.Setup(repo => repo.GetTrainingPlanByIdAsync(trainingPlanId))
                                       .ReturnsAsync(trainingPlan);

            _workoutRepositoryMock.Setup(repo => repo.AddWorkoutsAsync(It.IsAny<List<Workout>>()))
                              .ReturnsAsync(new List<Workout>());

            var result = await _workoutService.CreateRepeatedWorkoutAsync(workout);

            Assert.NotNull(result);
            Assert.Empty(result);

            _trainingPlanRepositoryMock.Verify(repo => repo.GetTrainingPlanByIdAsync(trainingPlanId), Times.Once);
            _workoutRepositoryMock.Verify(repo => repo.AddWorkoutsAsync(It.IsAny<List<Workout>>()), Times.Once);
        }
    }
}
