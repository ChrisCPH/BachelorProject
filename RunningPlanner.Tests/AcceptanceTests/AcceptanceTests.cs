using System.Net;
using System.Net.Http.Json;
using RunningPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Net.Http.Headers;
using System.Text;
using System.IdentityModel.Tokens.Jwt;

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

        // Test for user story 8 (create running route user story)
        [Fact]
        public async Task CreateRunningRoute_WithValidData_ReturnsCreated()
        {
            await AuthorizeClientAsync();

            var route = new
            {
                ID = "",
                Name = "Morning Run",
                Geometry = new
                {
                    Type = "LineString",
                    Coordinates = new List<List<double>>
                    {
                        new() { 12.4924, 41.8902 },
                        new() { 12.4964, 41.9028 }
                    }
                },
                CreatedAt = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                DistanceKm = 5.2
            };

            var response = await _client.PostAsJsonAsync("api/runningroute/add", route);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var created = await response.Content.ReadFromJsonAsync<RunningRoute>();
            Assert.NotNull(created);
            Assert.Equal("Morning Run", created.Name);
            Assert.Equal("LineString", created.Geometry.Type);
            Assert.Equal(2, created.Geometry.Coordinates.Count);
            Assert.Equal(5.2, created.DistanceKm);
        }

        public int ExtractUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserID");
            if (userIdClaim == null)
                throw new InvalidOperationException("UserID claim not found in token");

            return int.Parse(userIdClaim.Value);
        }

        // Test for user story 9 (add user to training plan user story)
        [Fact]
        public async Task AddUserToTrainingPlan_WithValidData_ReturnsOk()
        {
            await AuthorizeClientAsync();

            var trainingPlanId = await CreateTrainingPlanForAuthorizedUserAsync();

            var otherUser = new { Email = "viewer@example.com", Password = "Viewer123!", UserName = "viewer" };
            await _client.PostAsJsonAsync("api/user/register", otherUser);

            var loginResponse = await _client.PostAsJsonAsync("api/user/login", new { otherUser.Email, otherUser.Password });
            var loginContent = await loginResponse.Content.ReadFromJsonAsync<LoginSuccessResponse>();

            var otherUserId = ExtractUserIdFromToken(loginContent!.Token);

            var response = await _client.PostAsync(
                $"api/user/addUserToTrainingPlan?userId={otherUserId}&id={trainingPlanId}&permission=editor",
                null);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains("added", message.ToLower());
        }

        // Test for user story 9 (add user to training plan user story)
        [Fact]
        public async Task AddUserToTrainingPlan_WithInvalidPermission_ReturnsBadRequest()
        {
            await AuthorizeClientAsync();

            var trainingPlanId = await CreateTrainingPlanForAuthorizedUserAsync();

            var otherUser = new { Email = "invalidperm@example.com", Password = "Password123!", UserName = "invalidperm" };
            await _client.PostAsJsonAsync("api/user/register", otherUser);

            var loginResponse = await _client.PostAsJsonAsync("api/user/login", new { otherUser.Email, otherUser.Password });
            var loginContent = await loginResponse.Content.ReadFromJsonAsync<LoginSuccessResponse>();
            var otherUserId = ExtractUserIdFromToken(loginContent!.Token);

            var invalidPermission = "superadmin";

            var response = await _client.PostAsync(
                $"api/user/addUserToTrainingPlan?userId={otherUserId}&id={trainingPlanId}&permission={invalidPermission}",
                null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains("invalid permission", message.ToLower());
        }

        // Test for user story 9 (add user to training plan user story)
        [Fact]
        public async Task AddUserToTrainingPlan_WithNonExistentUser_ReturnsBadRequest()
        {
            await AuthorizeClientAsync();

            var trainingPlanId = await CreateTrainingPlanForAuthorizedUserAsync();

            var nonExistentUserId = 99999;

            var response = await _client.PostAsync(
                $"api/user/addUserToTrainingPlan?userId={nonExistentUserId}&id={trainingPlanId}&permission=viewer",
                null);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var message = await response.Content.ReadAsStringAsync();
            Assert.Contains("user or training plan not found.", message.ToLower());
        }

        // Test for user story 10 (create comment user story)
        [Fact]
        public async Task CreateCommentOnWorkout_WithValidRunId_ReturnsCreated()
        {
            await AuthorizeClientAsync();

            var trainingPlanId = await CreateTrainingPlanForAuthorizedUserAsync();

            var workout = new
            {
                TrainingPlanID = trainingPlanId,
                Type = "Strength",
                WeekNumber = 2,
                DayOfWeek = (DayOfWeek)3,
            };

            var workoutResponse = await _client.PostAsJsonAsync("api/workout/add", workout);
            workoutResponse.EnsureSuccessStatusCode();
            var createdWorkout = await workoutResponse.Content.ReadFromJsonAsync<Workout>();

            var comment = new
            {
                createdWorkout!.WorkoutID,
                Text = "Great job on this workout!",
                CreatedAt = DateTime.UtcNow
            };

            var commentResponse = await _client.PostAsJsonAsync("api/comment/add", comment);

            Assert.Equal(HttpStatusCode.Created, commentResponse.StatusCode);

            var createdComment = await commentResponse.Content.ReadFromJsonAsync<Comment>();
            Assert.NotNull(createdComment);
            Assert.Equal(comment.Text, createdComment.Text);
            Assert.Equal(createdWorkout.WorkoutID, createdComment.WorkoutID);
            Assert.True((DateTime.UtcNow - createdComment.CreatedAt).TotalSeconds < 60);
        }

        // Test for user story 10 (create comment user story)
        [Fact]
        public async Task CreateCommentOnRun_WithValidRunId_ReturnsCreated()
        {
            await AuthorizeClientAsync();

            var trainingPlanId = await CreateTrainingPlanForAuthorizedUserAsync();

            var run = new
            {
                TrainingPlanID = trainingPlanId,
                Type = "Easy",
                WeekNumber = 2,
                DayOfWeek = (DayOfWeek)3,
            };

            var runResponse = await _client.PostAsJsonAsync("api/run/add", run);
            runResponse.EnsureSuccessStatusCode();
            var createdrun = await runResponse.Content.ReadFromJsonAsync<Run>();

            var comment = new
            {
                createdrun!.RunID,
                Text = "Great job on this run!",
                CreatedAt = DateTime.UtcNow
            };

            var commentResponse = await _client.PostAsJsonAsync("api/comment/add", comment);

            Assert.Equal(HttpStatusCode.Created, commentResponse.StatusCode);

            var createdComment = await commentResponse.Content.ReadFromJsonAsync<Comment>();
            Assert.NotNull(createdComment);
            Assert.Equal(comment.Text, createdComment.Text);
            Assert.Equal(createdrun.RunID, createdComment.RunID);
            Assert.True((DateTime.UtcNow - createdComment.CreatedAt).TotalSeconds < 60);
        }

        // Test for user story 11 (update training plan user story)
        [Fact]
        public async Task UpdateTrainingPlan_WithValidDataAndPermission_ReturnsOk()
        {
            await AuthorizeClientAsync();

            var trainingPlanId = await CreateTrainingPlanForAuthorizedUserAsync();

            var getResponse = await _client.GetAsync($"api/trainingplan/{trainingPlanId}");
            getResponse.EnsureSuccessStatusCode();
            var existingTrainingPlan = await getResponse.Content.ReadFromJsonAsync<TrainingPlan>();
            Assert.NotNull(existingTrainingPlan);

            existingTrainingPlan!.Name = "Updated Plan Name";

            var updateResponse = await _client.PutAsJsonAsync("api/trainingplan/update", existingTrainingPlan);

            Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

            var updatedTrainingPlan = await updateResponse.Content.ReadFromJsonAsync<TrainingPlan>();
            Assert.NotNull(updatedTrainingPlan);
            Assert.Equal(existingTrainingPlan.Name, updatedTrainingPlan.Name);
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