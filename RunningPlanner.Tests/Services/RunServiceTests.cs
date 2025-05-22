using Moq;
using RunningPlanner.Models;
using RunningPlanner.Repositories;
using RunningPlanner.Services;

namespace RunningPlanner.Tests.Services
{
    public class RunServiceTests
    {
        private readonly Mock<IRunRepository> _runRepositoryMock;
        private readonly Mock<ITrainingPlanRepository> _trainingPlanRepositoryMock;
        private readonly RunService _runService;

        public RunServiceTests()
        {
            _trainingPlanRepositoryMock = new Mock<ITrainingPlanRepository>();
            _runRepositoryMock = new Mock<IRunRepository>();
            _runService = new RunService(_runRepositoryMock.Object, _trainingPlanRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateRunAsync_ShouldReturnCreatedRun_WhenRunIsValid()
        {
            var run = new Run { RunID = 1, DayOfWeek = (DayOfWeek)1, WeekNumber = 1 };
            _runRepositoryMock.Setup(repo => repo.AddRunAsync(run)).ReturnsAsync(run);

            var result = await _runService.CreateRunAsync(run);

            Assert.Equal(run, result);
            _runRepositoryMock.Verify(repo => repo.AddRunAsync(run), Times.Once);
        }

        [Fact]
        public async Task CreateRunAsync_ShouldThrowArgumentNullException_WhenRunIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _runService.CreateRunAsync(null!));
        }

        [Fact]
        public async Task GetRunByIdAsync_ShouldReturnRun_WhenRunExists()
        {
            var run = new Run { RunID = 1 };
            _runRepositoryMock.Setup(repo => repo.GetRunByIdAsync(run.RunID)).ReturnsAsync(run);

            var result = await _runService.GetRunByIdAsync(run.RunID);

            Assert.Equal(run, result);
            _runRepositoryMock.Verify(repo => repo.GetRunByIdAsync(run.RunID), Times.Once);
        }

        [Fact]
        public async Task GetAllRunsByTrainingPlanAsync_ShouldReturnRuns_WhenRunsExist()
        {
            var trainingPlanId = 1;
            var runs = new List<Run> { new Run { RunID = 1 }, new Run { RunID = 2 } };
            _runRepositoryMock.Setup(repo => repo.GetAllRunsByTrainingPlanAsync(trainingPlanId)).ReturnsAsync(runs);

            var result = await _runService.GetAllRunsByTrainingPlanAsync(trainingPlanId);

            Assert.Equal(runs, result);
            _runRepositoryMock.Verify(repo => repo.GetAllRunsByTrainingPlanAsync(trainingPlanId), Times.Once);
        }

        [Fact]
        public async Task UpdateRunAsync_ShouldReturnUpdatedRun_WhenRunIsValid()
        {
            var run = new Run { RunID = 1 };
            _runRepositoryMock.Setup(repo => repo.UpdateRunAsync(run)).ReturnsAsync(run);

            var result = await _runService.UpdateRunAsync(run);

            Assert.Equal(run, result);
            _runRepositoryMock.Verify(repo => repo.UpdateRunAsync(run), Times.Once);
        }

        [Fact]
        public async Task UpdateRunAsync_ShouldThrowArgumentNullException_WhenRunIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _runService.UpdateRunAsync(null!));
        }

        [Fact]
        public async Task DeleteRunAsync_ShouldReturnTrue_WhenRunIsDeleted()
        {
            var runId = 1;
            _runRepositoryMock.Setup(repo => repo.DeleteRunAsync(runId)).ReturnsAsync(true);

            var result = await _runService.DeleteRunAsync(runId);

            Assert.True(result);
            _runRepositoryMock.Verify(repo => repo.DeleteRunAsync(runId), Times.Once);
        }

        [Fact]
        public async Task CreateRepeatedRunAsync_ShouldCreateRunsForEachWeek_WhenRunAndTrainingPlanAreValid()
        {
            var trainingPlanId = 123;
            var trainingPlan = new TrainingPlan { TrainingPlanID = trainingPlanId, Duration = 4 };
            var inputRun = new Run
            {
                TrainingPlanID = trainingPlanId,
                Type = "Easy",
                DayOfWeek = DayOfWeek.Monday,
                TimeOfDay = new TimeSpan(7, 0, 0),
                Distance = 5,
                Duration = 3000,
                Pace = 300,
                Notes = "Test notes",
                Completed = false
            };

            _trainingPlanRepositoryMock.Setup(repo => repo.GetTrainingPlanByIdAsync(trainingPlanId))
                                       .ReturnsAsync(trainingPlan);

            List<Run>? runsAdded = null;
            _runRepositoryMock.Setup(repo => repo.AddRunsAsync(It.IsAny<List<Run>>()))
                              .Callback<List<Run>>(runs => runsAdded = runs)
                              .ReturnsAsync((List<Run> runs) => runs);

            var result = await _runService.CreateRepeatedRunAsync(inputRun);

            Assert.NotNull(result);
            Assert.Equal(trainingPlan.Duration, result.Count);
            Assert.NotNull(runsAdded);
            Assert.Equal(trainingPlan.Duration, runsAdded!.Count);

            for (int week = 1; week <= trainingPlan.Duration; week++)
            {
                var runForWeek = runsAdded.FirstOrDefault(r => r.WeekNumber == week);
                Assert.NotNull(runForWeek);
                Assert.Equal(inputRun.TrainingPlanID, runForWeek.TrainingPlanID);
                Assert.Equal(inputRun.Type, runForWeek.Type);
                Assert.Equal(inputRun.DayOfWeek, runForWeek.DayOfWeek);
                Assert.Equal(inputRun.TimeOfDay, runForWeek.TimeOfDay);
                Assert.Equal(inputRun.Distance, runForWeek.Distance);
                Assert.Equal(inputRun.Duration, runForWeek.Duration);
                Assert.Equal(inputRun.Pace, runForWeek.Pace);
                Assert.Equal(inputRun.Notes, runForWeek.Notes);
                Assert.Equal(inputRun.Completed, runForWeek.Completed);
                Assert.True((DateTime.UtcNow - runForWeek.CreatedAt).TotalSeconds < 5);
            }

            _trainingPlanRepositoryMock.Verify(repo => repo.GetTrainingPlanByIdAsync(trainingPlanId), Times.Once);
            _runRepositoryMock.Verify(repo => repo.AddRunsAsync(It.IsAny<List<Run>>()), Times.Once);
        }

        [Fact]
        public async Task CreateRepeatedRunAsync_ShouldThrowArgumentNullException_WhenRunIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _runService.CreateRepeatedRunAsync(null!));
        }

        [Fact]
        public async Task CreateRepeatedRunAsync_ShouldThrowArgumentException_WhenTrainingPlanNotFound()
        {
            var run = new Run { TrainingPlanID = 999 };

            _trainingPlanRepositoryMock.Setup(repo => repo.GetTrainingPlanByIdAsync(run.TrainingPlanID))
                                       .ReturnsAsync((TrainingPlan?)null);

            var ex = await Assert.ThrowsAsync<ArgumentException>(() => _runService.CreateRepeatedRunAsync(run));
            Assert.Equal("Training plan not found", ex.Message);
        }

        [Fact]
        public async Task CreateRepeatedRunAsync_ShouldReturnEmptyList_WhenTrainingPlanDurationIsZero()
        {
            var trainingPlanId = 123;
            var trainingPlan = new TrainingPlan { TrainingPlanID = trainingPlanId, Duration = 0 };
            var run = new Run { TrainingPlanID = trainingPlanId };

            _trainingPlanRepositoryMock.Setup(repo => repo.GetTrainingPlanByIdAsync(trainingPlanId))
                                       .ReturnsAsync(trainingPlan);

            _runRepositoryMock.Setup(repo => repo.AddRunsAsync(It.IsAny<List<Run>>()))
                              .ReturnsAsync(new List<Run>());

            var result = await _runService.CreateRepeatedRunAsync(run);

            Assert.NotNull(result);
            Assert.Empty(result);

            _trainingPlanRepositoryMock.Verify(repo => repo.GetTrainingPlanByIdAsync(trainingPlanId), Times.Once);
            _runRepositoryMock.Verify(repo => repo.AddRunsAsync(It.IsAny<List<Run>>()), Times.Once);
        }
    }
}
