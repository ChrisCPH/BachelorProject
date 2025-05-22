using System.Net;
using System.Net.Http.Json;
using RunningPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Net.Http.Headers;
using System.Text;

namespace RunningPlanner.Tests
{
    public class AcceptanceTests : AcceptanceTestBase
    {
        public AcceptanceTests(CustomWebApplicationFactory factory)
            : base(factory)
        {
        }

        private async Task AuthorizeClientAsync()
        {
            var user = new { Email = "runner2@example.com", Password = "Valid123!", UserName = "runner2" };
            await _client.PostAsJsonAsync("api/user/register", user);

            var loginResponse = await _client.PostAsJsonAsync("api/user/login", new { user.Email, user.Password });
            var loginContent = await loginResponse.Content.ReadFromJsonAsync<LoginSuccessResponse>();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", loginContent!.Token);
        }

        private async Task<int> CreateTrainingPlanForAuthorizedUserAsync()
        {
            await AuthorizeClientAsync();

            var trainingPlan = new
            {
                Name = "Test Plan",
                DurationWeeks = 4
            };

            var response = await _client.PostAsJsonAsync("api/trainingplan/add", trainingPlan);
            response.EnsureSuccessStatusCode();

            var created = await response.Content.ReadFromJsonAsync<TrainingPlan>();
            return created!.TrainingPlanID;
        }

        // Test for user story 1 (registration user story)
        [Fact]
        public async Task Register_WithValidData_ReturnsCreated()
        {
            var user = new
            {
                Email = "newuser@example.com",
                Password = "password1",
                UserName = "uniqueuser123"
            };

            var response = await _client.PostAsJsonAsync("api/user/register", user);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var createdUser = await response.Content.ReadFromJsonAsync<User>();
            Assert.NotNull(createdUser);
            Assert.Equal(user.Email, createdUser.Email);
            Assert.Equal(user.UserName, createdUser.UserName);
        }

        // Test for user story 1 (registration user story)
        [Fact]
        public async Task Register_WithInvalidPassword_ReturnsBadRequest()
        {
            var user = new
            {
                Email = "badpassword@example.com",
                Password = "short",
                UserName = "userwithbadpass"
            };

            var response = await _client.PostAsJsonAsync("api/user/register", user);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(error);
            Assert.Contains("Password must be at least 8 characters long and contain at least one number", error.Message);
        }

        // Test for user story 1 (registration user story)
        [Fact]
        public async Task Register_WithDuplicateEmail_ReturnsConflict()
        {
            var user = new
            {
                Email = "existinguser@example.com",
                Password = "validpass1",
                UserName = "user"
            };

            var firstResponse = await _client.PostAsJsonAsync("api/user/register", user);
            Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

            var usertwo = new
            {
                Email = "existinguser@example.com",
                Password = "validpass1",
                UserName = "usertwo"
            };

            var secondResponse = await _client.PostAsJsonAsync("api/user/register", usertwo);
            Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

            var error = await secondResponse.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(error);
            Assert.Equal($"A user with the email '{usertwo.Email}' already exists.", error.Message);
        }

        // Test for user story 1 (registration user story)
        [Fact]
        public async Task Register_WithDuplicateUsername_ReturnsConflict()
        {
            var user = new
            {
                Email = "user@example.com",
                Password = "validpass1",
                UserName = "existinguser"
            };

            var firstResponse = await _client.PostAsJsonAsync("api/user/register", user);
            Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

            var usertwo = new
            {
                Email = "usertwo@example.com",
                Password = "validpass1",
                UserName = "existinguser"
            };

            var secondResponse = await _client.PostAsJsonAsync("api/user/register", usertwo);
            Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

            var error = await secondResponse.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(error);
            Assert.Equal($"A user with the username '{usertwo.UserName}' already exists.", error.Message);
        }

        // Test for user story 2 (login user story)
        [Fact]
        public async Task Login_WithValidCredentials_ReturnsOk()
        {
            var registerUser = new
            {
                Email = "runner@example.com",
                Password = "StrongPass123",
                UserName = "runner01"
            };

            // First, register the user (assuming test database is clean)
            var registerResponse = await _client.PostAsJsonAsync("api/user/register", registerUser);
            Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

            var loginRequest = new
            {
                Email = "runner@example.com",
                Password = "StrongPass123"
            };

            var loginResponse = await _client.PostAsJsonAsync("api/user/login", loginRequest);
            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var loginContent = await loginResponse.Content.ReadFromJsonAsync<LoginSuccessResponse>();
            Assert.NotNull(loginContent);
            Assert.Equal("Login successful", loginContent.Message);
            Assert.False(string.IsNullOrEmpty(loginContent.Token));
        }

        // Test for user story 2 (login user story)
        [Fact]
        public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
        {
            var registerUser = new
            {
                Email = "runner2@example.com",
                Password = "CorrectPass123",
                UserName = "runner02"
            };

            await _client.PostAsJsonAsync("api/user/register", registerUser);

            var loginRequest = new
            {
                Email = "runner2@example.com",
                Password = "WrongPass456"
            };

            var loginResponse = await _client.PostAsJsonAsync("api/user/login", loginRequest);
            Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);

            var error = await loginResponse.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(error);
            Assert.Equal("Incorrect password.", error.Message);
        }

        // Test for user story 2 (login user story)
        [Fact]
        public async Task Login_WithMissingEmail_ReturnsBadRequest()
        {
            var loginRequest = new
            {
                Email = "",
                Password = "SomePass123"
            };

            var response = await _client.PostAsJsonAsync("api/user/login", loginRequest);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(error);
            Assert.Equal("Email and password are required.", error.Message);
        }

        // Test for user story 2 (login user story)
        [Fact]
        public async Task Login_WithMissingPassword_ReturnsBadRequest()
        {
            var loginRequest = new
            {
                Email = "runner3@example.com",
                Password = ""
            };

            var response = await _client.PostAsJsonAsync("api/user/login", loginRequest);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            Assert.NotNull(error);
            Assert.Equal("Email and password are required.", error.Message);
        }

        // Test for user story 3 (create training plan user story)
        [Fact]
        public async Task CreateTrainingPlan_WithValidData_ReturnsCreated()
        {
            await AuthorizeClientAsync();

            var trainingPlan = new
            {
                Name = "Marathon Prep",
                Duration = 12
            };

            var response = await _client.PostAsJsonAsync("api/trainingplan/add", trainingPlan);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<TrainingPlan>();
            Assert.NotNull(created);
            Assert.Equal("Marathon Prep", created.Name);
            Assert.Equal(12, created.Duration);
        }

        // Test for user story 3 (create training plan user story)
        [Fact]
        public async Task CreateTrainingPlan_MissingName_ReturnsBadRequest()
        {
            await AuthorizeClientAsync();

            var trainingPlan = new
            {
                Name = "",
                DurationWeeks = 8
            };

            var response = await _client.PostAsJsonAsync("api/trainingplan/add", trainingPlan);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test for user story 3 (create training plan user story)
        [Fact]
        public async Task CreateTrainingPlan_NullBody_ReturnsBadRequest()
        {
            await AuthorizeClientAsync();

            var content = new StringContent("", Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("api/trainingplan/add", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test for user story 4 (create run user story)
        [Fact]
        public async Task CreateRun_WithValidData_ReturnsCreated()
        {
            var trainingPlanId = await CreateTrainingPlanForAuthorizedUserAsync();

            var run = new
            {
                TrainingPlanID = trainingPlanId,
                WeekNumber = 2,
                DayOfWeek = (DayOfWeek)3,
            };

            var response = await _client.PostAsJsonAsync("api/run/add", run);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<Run>();
            Assert.NotNull(created);
            Assert.Equal(2, created.WeekNumber);
            Assert.Equal((DayOfWeek)3, created.DayOfWeek);
        }

        // Test for user story 4 (create run user story)
        [Fact]
        public async Task CreateRun_WithoutDay_ReturnsBadRequest()
        {
            var trainingPlanId = await CreateTrainingPlanForAuthorizedUserAsync();

            var run = new
            {
                TrainingPlanID = trainingPlanId,
                WeekNumber = 3,
            };

            var response = await _client.PostAsJsonAsync("api/run/add", run);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test for user story 4 (create run user story)
        [Fact]
        public async Task CreateRun_WithoutWeek_ReturnsBadRequest()
        {
            var trainingPlanId = await CreateTrainingPlanForAuthorizedUserAsync();

            var run = new
            {
                TrainingPlanID = trainingPlanId,
                DayOfWeek = (DayOfWeek)3,
            };

            var response = await _client.PostAsJsonAsync("api/run/add", run);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test for user story 4 (create run user story)
        [Fact]
        public async Task CreateRun_NullBody_ReturnsBadRequest()
        {
            await AuthorizeClientAsync();

            var content = new StringContent("", Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("api/run/add", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test for user story 5 (create workout user story)
        [Fact]
        public async Task CreateWorkout_WithValidData_ReturnsCreated()
        {
            var trainingPlanId = await CreateTrainingPlanForAuthorizedUserAsync();

            var workout = new
            {
                TrainingPlanID = trainingPlanId,
                Type = "Strength",
                WeekNumber = 2,
                DayOfWeek = (DayOfWeek)3,
            };

            var response = await _client.PostAsJsonAsync("api/workout/add", workout);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<Workout>();
            Assert.NotNull(created);
            Assert.Equal(2, created.WeekNumber);
            Assert.Equal((DayOfWeek)3, created.DayOfWeek);
        }

        // Test for user story 5 (create workout user story)
        [Fact]
        public async Task CreateWorkout_WithoutDay_ReturnsBadRequest()
        {
            var trainingPlanId = await CreateTrainingPlanForAuthorizedUserAsync();

            var workout = new
            {
                TrainingPlanID = trainingPlanId,
                WeekNumber = 3,
            };

            var response = await _client.PostAsJsonAsync("api/workout/add", workout);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test for user story 5 (create workout user story)
        [Fact]
        public async Task CreateWorkout_WithoutWeek_ReturnsBadRequest()
        {
            var trainingPlanId = await CreateTrainingPlanForAuthorizedUserAsync();

            var workout = new
            {
                TrainingPlanID = trainingPlanId,
                DayOfWeek = (DayOfWeek)3,
            };

            var response = await _client.PostAsJsonAsync("api/workout/add", workout);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Test for user story 5 (create workout user story)
        [Fact]
        public async Task CreateWorkout_NullBody_ReturnsBadRequest()
        {
            await AuthorizeClientAsync();

            var content = new StringContent("", Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("api/workout/add", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }


    }
    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }
    public class LoginSuccessResponse
    {
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}