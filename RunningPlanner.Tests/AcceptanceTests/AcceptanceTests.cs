using System.Net;
using System.Net.Http.Json;
using RunningPlanner.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace RunningPlanner.Tests
{
    public class AcceptanceTests : AcceptanceTestBase
    {
        public AcceptanceTests(CustomWebApplicationFactory factory)
            : base(factory)
        {
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
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
    }
}