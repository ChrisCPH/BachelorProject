using RunningPlanner.Models;
using RunningPlanner.Repositories;

namespace RunningPlanner.Tests
{
    public class TrainingPlanRepositoryTests : TestBase
    {
        [Fact]
        public async Task AddTrainingPlanAsync_ShouldAddTrainingPlan()
        {
            var context = GetInMemoryDbContext();
            var repo = new TrainingPlanRepository(context);

            var plan = new TrainingPlan
            {
                Name = "Marathon Prep",
                Event = "Marathon",
                GoalTime = "3:30"
            };

            var result = await repo.AddTrainingPlanAsync(plan);

            Assert.NotNull(result);
            Assert.Equal("Marathon Prep", result.Name);
            Assert.Single(context.TrainingPlan);
        }

        [Fact]
        public async Task GetTrainingPlanByIdAsync_ShouldReturnPlan_WithUserLink()
        {
            var context = GetInMemoryDbContext();

            var user = new User { UserID = 1, Email = "user@a.com" };
            var plan = new TrainingPlan { TrainingPlanID = 1, Name = "Plan X" };
            var link = new UserTrainingPlan
            {
                UserID = 1,
                TrainingPlanID = 1,
                Permission = "owner",
                User = user
            };

            context.User.Add(user);
            context.TrainingPlan.Add(plan);
            context.UserTrainingPlan.Add(link);
            await context.SaveChangesAsync();

            var repo = new TrainingPlanRepository(context);
            var result = await repo.GetTrainingPlanByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Plan X", result!.Name);
            Assert.Single(result.UserTrainingPlans);
            Assert.Equal("user@a.com", result.UserTrainingPlans.First().User.Email);
        }

        [Fact]
        public async Task GetAllTrainingPlansByUserAsync_ShouldReturnPlans()
        {
            var context = GetInMemoryDbContext();

            var user = new User { UserID = 1 };
            var plan1 = new TrainingPlan { TrainingPlanID = 1, Name = "Week 1" };
            var plan2 = new TrainingPlan { TrainingPlanID = 2, Name = "Week 2" };

            var link1 = new UserTrainingPlan { UserID = 1, TrainingPlanID = 1, TrainingPlan = plan1 };
            var link2 = new UserTrainingPlan { UserID = 1, TrainingPlanID = 2, TrainingPlan = plan2 };

            context.User.Add(user);
            context.TrainingPlan.AddRange(plan1, plan2);
            context.UserTrainingPlan.AddRange(link1, link2);
            await context.SaveChangesAsync();

            var repo = new TrainingPlanRepository(context);
            var result = await repo.GetAllTrainingPlansByUserAsync(1);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Count);
            Assert.Contains(result, p => p.Name == "Week 1");
            Assert.Contains(result, p => p.Name == "Week 2");
        }

        [Fact]
        public async Task UpdateTrainingPlanAsync_ShouldUpdateName()
        {
            var context = GetInMemoryDbContext();

            var plan = new TrainingPlan { TrainingPlanID = 1, Name = "Old Name" };
            context.TrainingPlan.Add(plan);
            await context.SaveChangesAsync();

            var repo = new TrainingPlanRepository(context);
            plan.Name = "New Name";
            var updated = await repo.UpdateTrainingPlanAsync(plan);

            Assert.Equal("New Name", updated.Name);
        }

        [Fact]
        public async Task DeleteTrainingPlanAsync_ShouldRemovePlan_AndUserLinks()
        {
            var context = GetInMemoryDbContext();

            var plan = new TrainingPlan { TrainingPlanID = 1 };
            var link = new UserTrainingPlan { TrainingPlanID = 1, UserID = 1 };

            context.TrainingPlan.Add(plan);
            context.UserTrainingPlan.Add(link);
            await context.SaveChangesAsync();

            var repo = new TrainingPlanRepository(context);
            var result = await repo.DeleteTrainingPlanAsync(1);

            Assert.True(result);
            Assert.Empty(context.TrainingPlan);
            Assert.Empty(context.UserTrainingPlan);
        }

        [Fact]
        public async Task DeleteTrainingPlanAsync_ShouldReturnFalse_IfNotFound()
        {
            var context = GetInMemoryDbContext();
            var repo = new TrainingPlanRepository(context);

            var result = await repo.DeleteTrainingPlanAsync(999);

            Assert.False(result);
        }

        [Fact]
        public async Task GetAllTrainingPlansWithPermissionsByUserAsync_ShouldReturnPlansWithPermissions()
        {
            var context = GetInMemoryDbContext();

            var user = new User { UserID = 1 };
            var plan1 = new TrainingPlan
            {
                TrainingPlanID = 1,
                Name = "Base Plan",
                Event = "Half Marathon",
            };
            var plan2 = new TrainingPlan
            {
                TrainingPlanID = 2,
                Name = "Advanced Plan",
                Event = "Marathon",
            };

            var link1 = new UserTrainingPlan
            {
                UserID = 1,
                TrainingPlanID = 1,
                TrainingPlan = plan1,
                Permission = "owner"
            };

            var link2 = new UserTrainingPlan
            {
                UserID = 1,
                TrainingPlanID = 2,
                TrainingPlan = plan2,
                Permission = "editor"
            };

            context.User.Add(user);
            context.TrainingPlan.AddRange(plan1, plan2);
            context.UserTrainingPlan.AddRange(link1, link2);
            await context.SaveChangesAsync();

            var repo = new TrainingPlanRepository(context);
            var result = await repo.GetAllTrainingPlansWithPermissionsByUserAsync(1);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Count);

            var basePlan = result.First(p => p.TrainingPlanID == 1);
            Assert.Equal("Base Plan", basePlan.Name);
            Assert.Equal("owner", basePlan.Permission);

            var advancedPlan = result.First(p => p.TrainingPlanID == 2);
            Assert.Equal("Advanced Plan", advancedPlan.Name);
            Assert.Equal("editor", advancedPlan.Permission);
        }

        [Fact]
        public async Task GetAllTrainingPlansWithPermissionsByUserAsync_ShouldReturnEmptyList_IfUserHasNoPlans()
        {
            var context = GetInMemoryDbContext();

            var user = new User { UserID = 1 };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var repo = new TrainingPlanRepository(context);
            var result = await repo.GetAllTrainingPlansWithPermissionsByUserAsync(1);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllTrainingPlansWithPermissionsByUserAsync_ShouldReturnEmptyList_IfUserDoesNotExist()
        {
            var context = GetInMemoryDbContext();

            var repo = new TrainingPlanRepository(context);
            var result = await repo.GetAllTrainingPlansWithPermissionsByUserAsync(999);

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}