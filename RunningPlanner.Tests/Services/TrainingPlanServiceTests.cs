using Moq;
using RunningPlanner.Models;
using RunningPlanner.Repositories;
using RunningPlanner.Services;

namespace RunningPlanner.Tests.Services
{
    public class TrainingPlanServiceTests
    {
        private readonly Mock<ITrainingPlanRepository> _trainingPlanRepositoryMock;
        private readonly Mock<IUserTrainingPlanRepository> _userTrainingPlanRepositoryMock;
        private readonly TrainingPlanService _trainingPlanService;

        public TrainingPlanServiceTests()
        {
            _trainingPlanRepositoryMock = new Mock<ITrainingPlanRepository>();
            _userTrainingPlanRepositoryMock = new Mock<IUserTrainingPlanRepository>();
            _trainingPlanService = new TrainingPlanService(_trainingPlanRepositoryMock.Object, _userTrainingPlanRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateTrainingPlanAsync_ShouldReturnCreatedTrainingPlan_WhenTrainingPlanIsValid()
        {
            var trainingPlan = new TrainingPlan { TrainingPlanID = 1 };
            int userId = 1;

            _trainingPlanRepositoryMock
                .Setup(repo => repo.AddTrainingPlanAsync(It.IsAny<TrainingPlan>()))
                .ReturnsAsync(trainingPlan);

            _userTrainingPlanRepositoryMock
                .Setup(repo => repo.AddUserTrainingPlanAsync(It.IsAny<UserTrainingPlan>()))
                .ReturnsAsync(new UserTrainingPlan
                {
                    UserID = userId,
                    TrainingPlanID = trainingPlan.TrainingPlanID,
                    Permission = "owner"
                });

            var result = await _trainingPlanService.CreateTrainingPlanAsync(trainingPlan, userId);

            Assert.Equal(trainingPlan, result);

            _trainingPlanRepositoryMock.Verify(repo => repo.AddTrainingPlanAsync(trainingPlan), Times.Once);

            _userTrainingPlanRepositoryMock.Verify(repo => repo.AddUserTrainingPlanAsync(
                It.Is<UserTrainingPlan>(utp =>
                    utp.UserID == userId &&
                    utp.TrainingPlanID == trainingPlan.TrainingPlanID &&
                    utp.Permission == "owner"
                )), Times.Once);
        }

        [Fact]
        public async Task CreateTrainingPlanAsync_ShouldThrowArgumentNullException_WhenTrainingPlanIsNull()
        {
            int userId = 1;
            await Assert.ThrowsAsync<ArgumentNullException>(() => _trainingPlanService.CreateTrainingPlanAsync(null!, userId));
        }

        [Fact]
        public async Task GetTrainingPlanByIdAsync_ShouldReturnTrainingPlan_WhenTrainingPlanExists()
        {
            var trainingPlan = new TrainingPlan { TrainingPlanID = 1 };
            _trainingPlanRepositoryMock.Setup(repo => repo.GetTrainingPlanByIdAsync(trainingPlan.TrainingPlanID)).ReturnsAsync(trainingPlan);

            var result = await _trainingPlanService.GetTrainingPlanByIdAsync(trainingPlan.TrainingPlanID);

            Assert.Equal(trainingPlan, result);
            _trainingPlanRepositoryMock.Verify(repo => repo.GetTrainingPlanByIdAsync(trainingPlan.TrainingPlanID), Times.Once);
        }

        [Fact]
        public async Task GetAllTrainingPlansByUserAsync_ShouldReturnTrainingPlans_WhenTrainingPlansExist()
        {
            int userId = 1;
            var trainingPlans = new List<TrainingPlan> { new TrainingPlan { TrainingPlanID = 1 }, new TrainingPlan { TrainingPlanID = 2 } };
            _trainingPlanRepositoryMock.Setup(repo => repo.GetAllTrainingPlansByUserAsync(userId)).ReturnsAsync(trainingPlans);

            var result = await _trainingPlanService.GetAllTrainingPlansByUserAsync(userId);

            Assert.Equal(trainingPlans, result);
            _trainingPlanRepositoryMock.Verify(repo => repo.GetAllTrainingPlansByUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task UpdateTrainingPlanAsync_ShouldReturnUpdatedTrainingPlan_WhenTrainingPlanIsValid()
        {
            var trainingPlan = new TrainingPlan { TrainingPlanID = 1 };
            _trainingPlanRepositoryMock.Setup(repo => repo.UpdateTrainingPlanAsync(trainingPlan)).ReturnsAsync(trainingPlan);

            var result = await _trainingPlanService.UpdateTrainingPlanAsync(trainingPlan);

            Assert.Equal(trainingPlan, result);
            _trainingPlanRepositoryMock.Verify(repo => repo.UpdateTrainingPlanAsync(trainingPlan), Times.Once);
        }

        [Fact]
        public async Task UpdateTrainingPlanAsync_ShouldThrowArgumentNullException_WhenTrainingPlanIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _trainingPlanService.UpdateTrainingPlanAsync(null!));
        }

        [Fact]
        public async Task DeleteTrainingPlanAsync_ShouldReturnTrue_WhenTrainingPlanIsDeleted()
        {
            int trainingPlanId = 1;
            _trainingPlanRepositoryMock.Setup(repo => repo.DeleteTrainingPlanAsync(trainingPlanId)).ReturnsAsync(true);

            var result = await _trainingPlanService.DeleteTrainingPlanAsync(trainingPlanId);

            Assert.True(result);
            _trainingPlanRepositoryMock.Verify(repo => repo.DeleteTrainingPlanAsync(trainingPlanId), Times.Once);
        }

        [Fact]
        public async Task GetAllTrainingPlansWithPermissionsByUserAsync_ShouldReturnPlansWithPermissions_WhenTheyExist()
        {
            int userId = 1;
            var plansWithPermissions = new List<TrainingPlanWithPermission>
    {
        new TrainingPlanWithPermission { TrainingPlanID = 1, Name = "Plan A", Permission = "owner" },
        new TrainingPlanWithPermission { TrainingPlanID = 2, Name = "Plan B", Permission = "editor" }
    };

            _trainingPlanRepositoryMock
                .Setup(repo => repo.GetAllTrainingPlansWithPermissionsByUserAsync(userId))
                .ReturnsAsync(plansWithPermissions);

            var result = await _trainingPlanService.GetAllTrainingPlansWithPermissionsByUserAsync(userId);

            Assert.Equal(plansWithPermissions, result);
            _trainingPlanRepositoryMock.Verify(repo => repo.GetAllTrainingPlansWithPermissionsByUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetAllTrainingPlansWithPermissionsByUserAsync_ShouldReturnEmptyList_WhenNoPlansExist()
        {
            int userId = 1;

            _trainingPlanRepositoryMock
                .Setup(repo => repo.GetAllTrainingPlansWithPermissionsByUserAsync(userId))
                .ReturnsAsync(new List<TrainingPlanWithPermission>());

            var result = await _trainingPlanService.GetAllTrainingPlansWithPermissionsByUserAsync(userId);

            Assert.Empty(result!);
            _trainingPlanRepositoryMock.Verify(repo => repo.GetAllTrainingPlansWithPermissionsByUserAsync(userId), Times.Once);
        }

        [Fact]
        public async Task GetAllTrainingPlansWithPermissionsByUserAsync_ShouldReturnNull_WhenRepositoryReturnsNull()
        {
            int userId = 1;

            _trainingPlanRepositoryMock
                .Setup(repo => repo.GetAllTrainingPlansWithPermissionsByUserAsync(userId))
                .ReturnsAsync((List<TrainingPlanWithPermission>?)null);

            var result = await _trainingPlanService.GetAllTrainingPlansWithPermissionsByUserAsync(userId);

            Assert.Null(result);
            _trainingPlanRepositoryMock.Verify(repo => repo.GetAllTrainingPlansWithPermissionsByUserAsync(userId), Times.Once);
        }

    }
}
