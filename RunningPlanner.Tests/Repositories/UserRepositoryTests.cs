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
        public async Task AddUserToTrainingPlanAsync_ShouldLinkUserAndPlan()
        {
            var context = GetInMemoryDbContext();

            var user = new User { UserID = 1, Email = "user@x.com" };
            var plan = new TrainingPlan { TrainingPlanID = 1, Name = "Plan A" };

            context.User.Add(user);
            context.TrainingPlan.Add(plan);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);
            var (success, message) = await repo.AddUserToTrainingPlanAsync(1, 1, "editor");

            Assert.True(success);
            Assert.Equal("User added to training plan.", message);
            Assert.Single(context.UserTrainingPlan);
        }

        [Fact]
        public async Task AddUserToTrainingPlanAsync_ShouldReturnFalse_IfUserOrPlanMissing()
        {
            var context = GetInMemoryDbContext();
            var repo = new UserRepository(context);

            var (success, message) = await repo.AddUserToTrainingPlanAsync(999, 888, "viewer");

            Assert.False(success);
            Assert.Equal("User or training plan not found.", message);
        }

        [Fact]
        public async Task AddUserToTrainingPlanAsync_ShouldNotDuplicateLink_IfPermissionSame()
        {
            var context = GetInMemoryDbContext();

            var user = new User { UserID = 1 };
            var plan = new TrainingPlan { TrainingPlanID = 1 };
            var link = new UserTrainingPlan { UserID = 1, TrainingPlanID = 1, Permission = "viewer" };

            context.User.Add(user);
            context.TrainingPlan.Add(plan);
            context.UserTrainingPlan.Add(link);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);
            var (success, message) = await repo.AddUserToTrainingPlanAsync(1, 1, "viewer");

            Assert.False(success);
            Assert.Equal("User already has this permission.", message);
            Assert.Single(context.UserTrainingPlan);
        }

        [Fact]
        public async Task AddUserToTrainingPlanAsync_ShouldUpdatePermission_IfDifferent()
        {
            var context = GetInMemoryDbContext();

            var user = new User { UserID = 1 };
            var plan = new TrainingPlan { TrainingPlanID = 1 };
            var link = new UserTrainingPlan { UserID = 1, TrainingPlanID = 1, Permission = "viewer" };

            context.User.Add(user);
            context.TrainingPlan.Add(plan);
            context.UserTrainingPlan.Add(link);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);
            var (success, message) = await repo.AddUserToTrainingPlanAsync(1, 1, "editor");

            Assert.True(success);
            Assert.Equal("Permission updated.", message);
            Assert.Single(context.UserTrainingPlan);
            Assert.Equal("editor", context.UserTrainingPlan.First().Permission);
        }

        [Fact]
        public async Task GetUserPermission_ShouldReturnPermission_WhenUserHasPermission()
        {
            var context = GetInMemoryDbContext();

            var user = new User { UserID = 1, Email = "user@x.com" };
            var plan = new TrainingPlan { TrainingPlanID = 1, Name = "Plan A" };
            var link = new UserTrainingPlan { UserID = 1, TrainingPlanID = 1, Permission = "editor" };

            context.User.Add(user);
            context.TrainingPlan.Add(plan);
            context.UserTrainingPlan.Add(link);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            var result = await repo.GetUserPermission(1, 1);

            Assert.Equal("editor", result);
        }

        [Fact]
        public async Task GetUserPermission_ShouldReturnNoPermissionFound_WhenUserHasNoPermission()
        {
            var context = GetInMemoryDbContext();

            var user = new User { UserID = 1, Email = "user@x.com" };
            var plan = new TrainingPlan { TrainingPlanID = 1, Name = "Plan A" };

            context.User.Add(user);
            context.TrainingPlan.Add(plan);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            var result = await repo.GetUserPermission(1, 1);

            Assert.Equal("No permission found", result);
        }

        [Fact]
        public async Task GetUserPermission_ShouldReturnNoPermissionFound_WhenUserIdOrTrainingPlanIdInvalid()
        {
            var context = GetInMemoryDbContext();

            var user = new User { UserID = 1, Email = "user@x.com" };
            var plan = new TrainingPlan { TrainingPlanID = 1, Name = "Plan A" };

            context.User.Add(user);
            context.TrainingPlan.Add(plan);
            await context.SaveChangesAsync();

            var repo = new UserRepository(context);

            var result = await repo.GetUserPermission(999, 999);

            Assert.Equal("No permission found", result);
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