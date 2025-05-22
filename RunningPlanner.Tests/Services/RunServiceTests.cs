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
    }
}
