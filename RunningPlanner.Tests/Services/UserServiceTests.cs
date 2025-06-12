using RunningPlanner.Models;
using RunningPlanner.Repositories;
using RunningPlanner.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Microsoft.AspNetCore.Identity;
namespace RunningPlanner.Tests
{
    public class UserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<ITrainingPlanRepository> _trainingPlanRepositoryMock;
        private readonly Mock<IUserTrainingPlanRepository> _userTrainingPlanRepositoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _trainingPlanRepositoryMock = new Mock<ITrainingPlanRepository>();
            _userTrainingPlanRepositoryMock = new Mock<IUserTrainingPlanRepository>();
            _configurationMock = new Mock<IConfiguration>();
            _userService = new UserService(_userRepositoryMock.Object, _trainingPlanRepositoryMock.Object, _userTrainingPlanRepositoryMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldCreateUser_WhenDataIsValid()
        {
            var user = new User
            {
                UserName = "TestUser",
                Email = "test@example.com",
                Password = "Password123"
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(user.Email))
                .ReturnsAsync((User?)null);

            _userRepositoryMock.Setup(repo => repo.AddUserAsync(It.IsAny<User>()))
                .ReturnsAsync((User u) => u);

            var result = await _userService.CreateUserAsync(user);

            Assert.NotNull(result);
            Assert.NotEqual("Password123", result.Password);
            _userRepositoryMock.Verify(repo => repo.AddUserAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrow_WhenEmailAlreadyExists()
        {
            var existingUser = new User { Email = "existing@example.com" };

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(existingUser.Email))
                .ReturnsAsync(existingUser);

            var newUser = new User
            {
                UserName = "NewUser",
                Email = "existing@example.com",
                Password = "Password123"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateUserAsync(newUser));
        }

        [Fact]
        public async Task CreateUserAsync_ShouldThrow_WhenUserNameAlreadyExists()
        {
            var existingUser = new User { UserName = "existingUserName" };

            _userRepositoryMock.Setup(repo => repo.GetUserByUsernameAsync(existingUser.UserName))
                .ReturnsAsync(existingUser);

            var newUser = new User
            {
                UserName = "existingUserName",
                Email = "test@email.com",
                Password = "Password123"
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.CreateUserAsync(newUser));
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnToken_WhenCredentialsAreValid()
        {
            var user = new User
            {
                UserID = 1,
                UserName = "TestUser",
                Email = "test@example.com",
                Password = new PasswordHasher<User>().HashPassword(null!, "Password123")
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(user.Email))
                .ReturnsAsync(user);

            _configurationMock.Setup(config => config.GetSection("Jwt")["Key"]).Returns("super_secret_key_12345_with_256_bits");
            _configurationMock.Setup(config => config.GetSection("Jwt")["Issuer"]).Returns("test_issuer");
            _configurationMock.Setup(config => config.GetSection("Jwt")["Audience"]).Returns("test_audience");
            _configurationMock.Setup(config => config.GetSection("Jwt")["ExpiresInMinutes"]).Returns("60");

            var token = await _userService.LoginAsync(user.Email, "Password123");

            Assert.False(string.IsNullOrEmpty(token));
        }

        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenUserNotFound()
        {
            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.LoginAsync("nonexistent@example.com", "Password123"));
        }

        [Fact]
        public async Task LoginAsync_ShouldThrow_WhenPasswordIncorrect()
        {
            var user = new User
            {
                UserID = 1,
                UserName = "TestUser",
                Email = "test@example.com",
                Password = new PasswordHasher<User>().HashPassword(null!, "CorrectPassword123")
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(user.Email))
                .ReturnsAsync(user);

            await Assert.ThrowsAsync<InvalidOperationException>(() => _userService.LoginAsync(user.Email, "WrongPassword"));
        }

        [Fact]
        public async Task AddUserToTrainingPlanAsync_ShouldAddNewLink_WhenNoExistingLink()
        {
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(1))
                .ReturnsAsync(new User { UserID = 1 });

            _trainingPlanRepositoryMock.Setup(repo => repo.GetTrainingPlanByIdAsync(1))
                .ReturnsAsync(new TrainingPlan { TrainingPlanID = 1 });

            _userTrainingPlanRepositoryMock.Setup(repo => repo.GetUserTrainingPlanAsync(1, 1))
                .ReturnsAsync((UserTrainingPlan?)null);

            _userTrainingPlanRepositoryMock.Setup(repo => repo.AddUserTrainingPlanAsync(It.IsAny<UserTrainingPlan>()))
                .ReturnsAsync((UserTrainingPlan utp) => utp);

            var result = await _userService.AddUserToTrainingPlanAsync(1, 1, "viewer");

            Assert.True(result.Success);
            Assert.Equal("User added to training plan.", result.Message);

            _userTrainingPlanRepositoryMock.Verify(repo => repo.AddUserTrainingPlanAsync(
                It.Is<UserTrainingPlan>(utp => utp.Permission == "viewer")), Times.Once);

            _userTrainingPlanRepositoryMock.Verify(repo => repo.UpdateUserTrainingPlanAsync(It.IsAny<UserTrainingPlan>()), Times.Never);
        }

        [Fact]
        public async Task AddUserToTrainingPlanAsync_ShouldReturnFalse_WhenPermissionIsInvalid()
        {
            var result = await _userService.AddUserToTrainingPlanAsync(1, 1, "admin");

            Assert.False(result.Success);
            Assert.Equal("Invalid permission.", result.Message);
            _userRepositoryMock.Verify(repo => repo.GetUserByIdAsync(It.IsAny<int>()), Times.Never);
            _trainingPlanRepositoryMock.Verify(repo => repo.GetTrainingPlanByIdAsync(It.IsAny<int>()), Times.Never);
            _userTrainingPlanRepositoryMock.Verify(repo => repo.GetUserTrainingPlanAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task AddUserToTrainingPlanAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(1))
                .ReturnsAsync((User?)null);

            var result = await _userService.AddUserToTrainingPlanAsync(1, 1, "viewer");

            Assert.False(result.Success);
            Assert.Equal("User not found.", result.Message);
            _trainingPlanRepositoryMock.Verify(repo => repo.GetTrainingPlanByIdAsync(It.IsAny<int>()), Times.Never);
            _userTrainingPlanRepositoryMock.Verify(repo => repo.GetUserTrainingPlanAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task AddUserToTrainingPlanAsync_ShouldReturnFalse_WhenTrainingPlanNotFound()
        {
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(1))
                .ReturnsAsync(new User { UserID = 1 });

            _trainingPlanRepositoryMock.Setup(repo => repo.GetTrainingPlanByIdAsync(1))
                .ReturnsAsync((TrainingPlan?)null);

            var result = await _userService.AddUserToTrainingPlanAsync(1, 1, "viewer");

            Assert.False(result.Success);
            Assert.Equal("Training plan not found.", result.Message);
            _userTrainingPlanRepositoryMock.Verify(repo => repo.GetUserTrainingPlanAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task AddUserToTrainingPlanAsync_ShouldReturnFalse_WhenUserHasOwnerPermission()
        {
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(1))
                .ReturnsAsync(new User { UserID = 1 });

            _trainingPlanRepositoryMock.Setup(repo => repo.GetTrainingPlanByIdAsync(1))
                .ReturnsAsync(new TrainingPlan { TrainingPlanID = 1 });

            _userTrainingPlanRepositoryMock.Setup(repo => repo.GetUserTrainingPlanAsync(1, 1))
                .ReturnsAsync(new UserTrainingPlan { UserID = 1, TrainingPlanID = 1, Permission = "owner" });

            var result = await _userService.AddUserToTrainingPlanAsync(1, 1, "editor");

            Assert.False(result.Success);
            Assert.Equal("User already has Owner permission.", result.Message);

            _userTrainingPlanRepositoryMock.Verify(repo => repo.UpdateUserTrainingPlanAsync(It.IsAny<UserTrainingPlan>()), Times.Never);
            _userTrainingPlanRepositoryMock.Verify(repo => repo.AddUserTrainingPlanAsync(It.IsAny<UserTrainingPlan>()), Times.Never);
        }

        [Fact]
        public async Task AddUserToTrainingPlanAsync_ShouldUpdatePermission_WhenExistingLinkAndNotOwner()
        {
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(1))
                .ReturnsAsync(new User { UserID = 1 });

            _trainingPlanRepositoryMock.Setup(repo => repo.GetTrainingPlanByIdAsync(1))
                .ReturnsAsync(new TrainingPlan { TrainingPlanID = 1 });

            var existingLink = new UserTrainingPlan { UserID = 1, TrainingPlanID = 1, Permission = "viewer" };
            _userTrainingPlanRepositoryMock.Setup(repo => repo.GetUserTrainingPlanAsync(1, 1))
                .ReturnsAsync(existingLink);

            _userTrainingPlanRepositoryMock.Setup(repo => repo.UpdateUserTrainingPlanAsync(It.IsAny<UserTrainingPlan>()))
                .ReturnsAsync((UserTrainingPlan updated) => updated);

            var result = await _userService.AddUserToTrainingPlanAsync(1, 1, "editor");

            Assert.True(result.Success);
            Assert.Equal("Permission updated.", result.Message);

            _userTrainingPlanRepositoryMock.Verify(repo => repo.UpdateUserTrainingPlanAsync(
                It.Is<UserTrainingPlan>(utp => utp.Permission == "editor")), Times.Once);

            _userTrainingPlanRepositoryMock.Verify(repo => repo.AddUserTrainingPlanAsync(It.IsAny<UserTrainingPlan>()), Times.Never);
        }

        [Fact]
        public async Task GetUserNameByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            var user = new UserAdd
            {
                UserID = 1,
                UserName = "TestUser",
            };

            _userRepositoryMock.Setup(repo => repo.GetUserNameByIdAsync(1))
                .ReturnsAsync(user);

            var result = await _userService.GetUserNameByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(user.UserName, result!.UserName);
        }

        [Fact]
        public async Task GetUserIdByNameAsync_ShouldReturnUserAdd_WhenUserExists()
        {
            var user = new UserAdd { UserID = 42, UserName = "Alice" };
            _userRepositoryMock
                .Setup(repo => repo.GetUserIdByUsernameAsync("Alice"))
                .ReturnsAsync(user);

            var result = await _userService.GetUserIdByNameAsync("Alice");

            Assert.NotNull(result);
            Assert.Equal(42, result!.UserID);
            Assert.Equal("Alice", result.UserName);
        }

        [Fact]
        public async Task GetUserIdByNameAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            _userRepositoryMock
                .Setup(repo => repo.GetUserIdByUsernameAsync("ghost"))
                .ReturnsAsync((UserAdd?)null);

            var result = await _userService.GetUserIdByNameAsync("ghost");

            Assert.Null(result);
        }
    }
}