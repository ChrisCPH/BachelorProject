using RunningPlanner.Models;
using RunningPlanner.Repositories;

namespace RunningPlanner.Tests
{
    public class RunRepositoryTests : TestBase
    {
        [Fact]
        public async Task AddRunAsync_ShouldAddRun()
        {
            var context = GetInMemoryDbContext();
            var repo = new RunRepository(context);

            var trainingPlan = new TrainingPlan { TrainingPlanID = 1, Name = "Marathon Plan" };
            context.TrainingPlan.Add(trainingPlan);
            await context.SaveChangesAsync();

            var run = new Run { RunID = 1, Distance = 5.0, TrainingPlanID = trainingPlan.TrainingPlanID };
            var result = await repo.AddRunAsync(run);

            Assert.NotNull(result);
            Assert.Equal(1, context.Run.Count());
            Assert.Equal(5.0, result.Distance);
        }

        [Fact]
        public async Task GetRunByIdAsync_ShouldReturnRun()
        {
            var context = GetInMemoryDbContext();
            var repo = new RunRepository(context);

            var trainingPlan = new TrainingPlan { TrainingPlanID = 1, Name = "10K Plan" };
            var run = new Run { RunID = 1, Distance = 5.0, TrainingPlanID = trainingPlan.TrainingPlanID };
            context.TrainingPlan.Add(trainingPlan);
            context.Run.Add(run);
            await context.SaveChangesAsync();

            var result = await repo.GetRunByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(5.0, result!.Distance);
        }

        [Fact]
        public async Task GetAllRunsByTrainingPlanAsync_ShouldReturnListOfRuns()
        {
            var context = GetInMemoryDbContext();
            var repo = new RunRepository(context);

            var trainingPlan = new TrainingPlan { TrainingPlanID = 1, Name = "Half Marathon" };
            var run1 = new Run { RunID = 1, Distance = 5.0, TrainingPlanID = trainingPlan.TrainingPlanID };
            var run2 = new Run { RunID = 2, Distance = 10, TrainingPlanID = trainingPlan.TrainingPlanID };

            context.TrainingPlan.Add(trainingPlan);
            context.Run.AddRange(run1, run2);
            await context.SaveChangesAsync();

            var result = await repo.GetAllRunsByTrainingPlanAsync(trainingPlan.TrainingPlanID);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Count);
            Assert.Contains(result, r => r.Distance == 5);
            Assert.Contains(result, r => r.Distance == 10);
        }

        [Fact]
        public async Task UpdateRunAsync_ShouldUpdateRun()
        {
            var context = GetInMemoryDbContext();
            var repo = new RunRepository(context);

            var trainingPlan = new TrainingPlan { TrainingPlanID = 1, Name = "Beginner Plan" };
            var run = new Run { RunID = 1, Distance = 5.0, TrainingPlanID = trainingPlan.TrainingPlanID };
            context.TrainingPlan.Add(trainingPlan);
            context.Run.Add(run);
            await context.SaveChangesAsync();

            run.Distance = 6;
            var updatedRun = await repo.UpdateRunAsync(run);

            Assert.Equal(6, updatedRun.Distance);
        }

        [Fact]
        public async Task DeleteRunAsync_ShouldRemoveRun()
        {
            var context = GetInMemoryDbContext();
            var repo = new RunRepository(context);

            var trainingPlan = new TrainingPlan { TrainingPlanID = 1, Name = "Training Plan" };
            var run = new Run { RunID = 1, Distance = 5.0, TrainingPlanID = trainingPlan.TrainingPlanID };
            context.TrainingPlan.Add(trainingPlan);
            context.Run.Add(run);
            await context.SaveChangesAsync();

            var result = await repo.DeleteRunAsync(1);

            Assert.True(result);
            Assert.Empty(context.Run);
        }

        [Fact]
        public async Task DeleteRunAsync_ShouldReturnFalse_IfRunNotFound()
        {
            var context = GetInMemoryDbContext();
            var repo = new RunRepository(context);

            var result = await repo.DeleteRunAsync(999);

            Assert.False(result);
        }
    }
}
