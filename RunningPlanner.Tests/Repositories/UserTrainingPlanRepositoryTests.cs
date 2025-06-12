using Microsoft.EntityFrameworkCore;
using RunningPlanner.Models;
using RunningPlanner.Repositories;

namespace RunningPlanner.Tests
{
    public class UserTrainingPlanRepositoryTests : TestBase
    {
        [Fact]
        public async Task GetUserTrainingPlanAsync_ReturnsUserTrainingPlan_WhenExists()
        {
            var context = GetInMemoryDbContext();
            var repo = new UserTrainingPlanRepository(context);

            var userTrainingPlan = new UserTrainingPlan
            {
                UserID = 1,
                TrainingPlanID = 2,
                Permission = "owner"
            };
            context.UserTrainingPlan.Add(userTrainingPlan);
            await context.SaveChangesAsync();

            var result = await repo.GetUserTrainingPlanAsync(1, 2);

            Assert.NotNull(result);
            Assert.Equal(1, result.UserID);
            Assert.Equal(2, result.TrainingPlanID);
            Assert.Equal("owner", result.Permission);
        }

        [Fact]
        public async Task AddUserTrainingPlanAsync_ShouldLinkUserToTrainingPlan()
        {
            var context = GetInMemoryDbContext();
            var repo = new UserTrainingPlanRepository(context);

            var user = new User { UserID = 1, Email = "test@run.com" };
            var plan = new TrainingPlan { Name = "5K Plan" };

            context.User.Add(user);
            context.TrainingPlan.Add(plan);
            await context.SaveChangesAsync();

            var userTrainingPlan = new UserTrainingPlan
            {
                UserID = user.UserID,
                TrainingPlanID = plan.TrainingPlanID,
                Permission = "owner"
            };

            var result = await repo.AddUserTrainingPlanAsync(userTrainingPlan);

            Assert.NotNull(result);
            Assert.Equal("owner", result.Permission);
            Assert.Single(context.UserTrainingPlan);
        }

        [Fact]
        public async Task UpdateUserTrainingPlanAsync_UpdatesAndReturnsUserTrainingPlan()
        {
            var context = GetInMemoryDbContext();
            var repo = new UserTrainingPlanRepository(context);

            var userTrainingPlan = new UserTrainingPlan
            {
                UserID = 3,
                TrainingPlanID = 4,
                Permission = "viewer"
            };
            context.UserTrainingPlan.Add(userTrainingPlan);
            await context.SaveChangesAsync();

            userTrainingPlan.Permission = "editor";

            var updatedPlan = await repo.UpdateUserTrainingPlanAsync(userTrainingPlan);

            var dbEntry = await context.UserTrainingPlan
                .FirstOrDefaultAsync(utp =>
                    utp.UserID == userTrainingPlan.UserID &&
                    utp.TrainingPlanID == userTrainingPlan.TrainingPlanID);

            Assert.Equal("editor", updatedPlan.Permission);
            Assert.Equal("editor", dbEntry!.Permission);
        }

    }
}