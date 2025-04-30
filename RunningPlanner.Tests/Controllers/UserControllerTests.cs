using Microsoft.AspNetCore.Mvc;
using Moq;
using RunningPlanner.Controllers;
using RunningPlanner.Models;
using RunningPlanner.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RunningPlanner.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserService> _userServiceMock;
        private readonly UserController _userController;

        public UserControllerTests()
        {
            _userServiceMock = new Mock<IUserService>();
            _userController = new UserController(_userServiceMock.Object);
        }

        [Fact]
        public async Task Register_ShouldReturnCreatedAtAction_WhenUserIsValid()
        {
            var user = new User { UserID = 1 };
            _userServiceMock.Setup(s => s.CreateUserAsync(user)).ReturnsAsync(user);

            var result = await _userController.Register(user);

            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_userController.GetUserById), createdAtActionResult.ActionName);
            Assert.Equal(user, createdAtActionResult.Value);
        }

        [Fact]
        public async Task Register_ShouldReturnBadRequest_WhenUserIsNull()
        {
            var result = await _userController.Register(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            var value = badRequestResult.Value!.ToString();

            Assert.Contains("User data is required.", value);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnOk_WhenUserExists()
        {
            var user = new User { UserID = 1 };
            _userServiceMock.Setup(s => s.GetUserByIdAsync(user.UserID)).ReturnsAsync(user);

            var result = await _userController.GetUserById(user.UserID);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(user, okResult.Value);
        }

        [Fact]
        public async Task GetUserById_ShouldReturnNotFound_WhenUserDoesNotExist()
        {
            _userServiceMock.Setup(s => s.GetUserByIdAsync(It.IsAny<int>())).ReturnsAsync((User?)null);

            var result = await _userController.GetUserById(1);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Login_ReturnsJwtToken_WhenCredentialsAreValid()
        {
            var loginRequest = new LoginRequest { Email = "test@example.com", Password = "password" };
            var token = "test_token";
            _userServiceMock.Setup(s => s.LoginAsync(loginRequest.Email, loginRequest.Password)).ReturnsAsync(token);

            var result = await _userController.Login(loginRequest);

            var actionResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(200, actionResult.StatusCode);
        }

        [Fact]
        public async Task Login_ShouldReturnBadRequest_WhenLoginRequestIsInvalid()
        {
            var result = await _userController.Login(null!);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            var value = badRequestResult.Value!.ToString();
            Assert.Contains("Email and password are required.", value);
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
        {
            var loginRequest = new LoginRequest { Email = "test@example.com", Password = "wrongpassword" };
            _userServiceMock.Setup(s => s.LoginAsync(loginRequest.Email, loginRequest.Password)).ReturnsAsync(string.Empty);

            var result = await _userController.Login(loginRequest);

            var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);

            var value = unauthorizedResult.Value!.ToString();

            Assert.Contains("Invalid email or password.", value);
        }


        [Fact]
        public async Task AddUserToTrainingPlan_ShouldReturnOk_WhenSuccessful()
        {
            int userId = 1, trainingPlanId = 2;
            string permission = "read";
            _userServiceMock.Setup(s => s.AddUserToTrainingPlanAsync(userId, trainingPlanId, permission)).ReturnsAsync(true);

            var result = await _userController.AddUserToTrainingPlan(userId, trainingPlanId, permission);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("User is now following the training plan", okResult.Value);
        }

        [Fact]
        public async Task AddUserToTrainingPlan_ShouldReturnBadRequest_WhenFailed()
        {
            int userId = 1, trainingPlanId = 2;
            string permission = "read";
            _userServiceMock.Setup(s => s.AddUserToTrainingPlanAsync(userId, trainingPlanId, permission)).ReturnsAsync(false);

            var result = await _userController.AddUserToTrainingPlan(userId, trainingPlanId, permission);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("Failed to follow training plan.", badRequestResult.Value);
        }
    }
}
