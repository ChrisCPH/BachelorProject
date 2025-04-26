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
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _configurationMock = new Mock<IConfiguration>();
            _userService = new UserService(_userRepositoryMock.Object, _configurationMock.Object);
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
        public async Task AddUserToTrainingPlanAsync_ShouldReturnTrue_WhenRepositoryReturnsTrue()
        {
            _userRepositoryMock.Setup(repo => repo.AddUserToTrainingPlanAsync(1, 1, "Owner"))
                .ReturnsAsync(true);

            var result = await _userService.AddUserToTrainingPlanAsync(1, 1, "Owner");

            Assert.True(result);
            _userRepositoryMock.Verify(repo => repo.AddUserToTrainingPlanAsync(1, 1, "Owner"), Times.Once);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUser_WhenUserExists()
        {
            var user = new User
            {
                UserID = 1,
                UserName = "TestUser",
                Email = "test@example.com",
                Password = "Password123"
            };

            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(1))
                .ReturnsAsync(user);

            var result = await _userService.GetUserByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(user.UserName, result!.UserName);
        }
    }

}