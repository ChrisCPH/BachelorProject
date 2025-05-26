using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using RunningPlanner.Models;
using Xunit;

namespace RunningPlanner.Tests
{
    public class SystemTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public SystemTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task UpdateTrainingPlan_WithAndWithoutPermission_EnforcesCorrectAccess()
        {
            // Arrange: Create User A and login
            var userA = new { Email = "usera@example.com", Password = "Valid123!", UserName = "usera" };
            await _client.PostAsJsonAsync("api/user/register", userA);
            var loginA = await _client.PostAsJsonAsync("api/user/login", new { userA.Email, userA.Password });
            var tokenA = (await loginA.Content.ReadFromJsonAsync<LoginSuccessResponse>())!.Token;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);

            // User A creates a training plan
            var trainingPlan = new { Name = "Plan A", Description = "Created by A" };
            var createResponse = await _client.PostAsJsonAsync("api/trainingplan/add", trainingPlan);
            createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createResponse.Content.ReadFromJsonAsync<TrainingPlan>();
            created.Should().NotBeNull();

            // Act 1: User A updates their own training plan
            var updatedPlan = new TrainingPlan
            {
                TrainingPlanID = created!.TrainingPlanID,
                Name = "Updated by A",
                Duration = 14,
            };
            var updateResponseA = await _client.PutAsJsonAsync("api/trainingplan/update", updatedPlan);

            // Assert 1: Should be allowed
            updateResponseA.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedResultA = await updateResponseA.Content.ReadFromJsonAsync<TrainingPlan>();
            updatedResultA!.Name.Should().Be("Updated by A");

            // Arrange: Create User B and login
            var userB = new { Email = "userb@example.com", Password = "Valid123!", UserName = "userb" };
            await _client.PostAsJsonAsync("api/user/register", userB);
            var loginB = await _client.PostAsJsonAsync("api/user/login", new { userB.Email, userB.Password });
            var tokenB = (await loginB.Content.ReadFromJsonAsync<LoginSuccessResponse>())!.Token;
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

            // Act 2: User B tries to update the same training plan
            var updateAsB = new TrainingPlan
            {
                TrainingPlanID = created!.TrainingPlanID,
                Name = "Updated by B",
                Duration = 12
            };
            var updateResponseB = await _client.PutAsJsonAsync("api/trainingplan/update", updateAsB);

            // Assert 2: Should be forbidden
            updateResponseB.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }


        [Fact]
        public async Task UpdateTrainingPlan_Returns401_IfUserNotAuthenticated()
        {
            var trainingPlan = new TrainingPlan { TrainingPlanID = 999, Name = "Should Fail", Duration = 10 };

            _client.DefaultRequestHeaders.Authorization = null; // Remove auth
            var response = await _client.PutAsJsonAsync("api/trainingplan/update", trainingPlan);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
