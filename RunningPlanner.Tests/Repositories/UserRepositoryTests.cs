using RunningPlanner.Models;
using RunningPlanner.Repositories;

namespace RunningPlanner.Tests
{
    public class UserRepositoryTests : TestBase
    {
        [Fact]
        public async Task AddUserAsync_ShouldAddUser()
        {
            var context = GetInMemoryDbContext();
            var repo = new UserRepository(context);
            var user = new User { Email = "test@example.com" };

            var result = await repo.AddUserAsync(user);

            Assert.Equal("test@example.com", result.Email);
            Assert.Single(context.User);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnCorrectUser()
        {
            var context = GetInMemoryDbContext();
            var user = new User { UserID = 1, Email = "a@b.com" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            var result = await repo.GetUserByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("a@b.com", result!.Email);
        }

        [Fact]
        public async Task GetUserNameByIdAsync_ShouldReturnCorrectUser()
        {
            var context = GetInMemoryDbContext();
            var user = new User { UserID = 1, UserName = "UserOne" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            var result = await repo.GetUserNameByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("UserOne", result!.UserName);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnCorrectUser()
        {
            var context = GetInMemoryDbContext();
            var user = new User { Email = "abc@xyz.com" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);
            var result = await repo.GetUserByEmailAsync("abc@xyz.com");

            Assert.NotNull(result);
            Assert.Equal("abc@xyz.com", result!.Email);
        }

        [Fact]
        public async Task GetUserIdByUsernameAsync_ShouldReturnUserAdd_WhenUserExists()
        {
            var context = GetInMemoryDbContext();

            var user = new User { UserID = 1, UserName = "testUser" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            var result = await repo.GetUserIdByUsernameAsync("testUser");

            Assert.NotNull(result);
            Assert.Equal(1, result!.UserID);
            Assert.Equal("testUser", result.UserName);
        }
    }
}