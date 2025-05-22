using Microsoft.EntityFrameworkCore;
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

        [Fact]
        public async Task RemoveRouteIdFromRunsAsync_ShouldNullifyRouteIdForMatchingRuns()
        {
            var context = GetInMemoryDbContext();
            var repo = new RunRepository(context);

            context.Run.AddRange(
                new Run { RunID = 1, RouteID = "route123" },
                new Run { RunID = 2, RouteID = "route123" },
                new Run { RunID = 3, RouteID = "otherRoute" }
            );
            await context.SaveChangesAsync();

            await repo.RemoveRouteIdFromRunsAsync("route123");

            var runs = await context.Run.ToListAsync();

            Assert.All(runs.Where(r => r.RunID == 1 || r.RunID == 2), run => Assert.Null(run.RouteID));
            Assert.Equal("otherRoute", runs.First(r => r.RunID == 3).RouteID);
        }

        [Fact]
        public async Task RemoveRouteIdFromRunsAsync_ShouldNotChangeIfNoMatch()
        {
            var context = GetInMemoryDbContext();
            var repo = new RunRepository(context);

            context.Run.AddRange(
                new Run { RunID = 1, RouteID = "someRoute" },
                new Run { RunID = 2, RouteID = "anotherRoute" }
            );
            await context.SaveChangesAsync();

            await repo.RemoveRouteIdFromRunsAsync("nonexistentRoute");

            var runs = await context.Run.ToListAsync();
            Assert.All(runs, run => Assert.NotNull(run.RouteID));
        }

        [Fact]
        public async Task AddRunsAsync_ShouldAddMultipleRuns()
        {
            var context = GetInMemoryDbContext();
            var repo = new RunRepository(context);

            var trainingPlan = new TrainingPlan { TrainingPlanID = 1, Name = "Marathon Plan" };
            context.TrainingPlan.Add(trainingPlan);
            await context.SaveChangesAsync();

            var runsToAdd = new List<Run>
            {
                new Run { RunID = 1, Distance = 5.0, TrainingPlanID = trainingPlan.TrainingPlanID },
                new Run { RunID = 2, Distance = 10.0, TrainingPlanID = trainingPlan.TrainingPlanID }
            };

            var result = await repo.AddRunsAsync(runsToAdd);

            Assert.NotNull(result);
            Assert.Equal(2, context.Run.Count());
            Assert.Contains(context.Run, r => r.Distance == 5.0);
            Assert.Contains(context.Run, r => r.Distance == 10.0);

            Assert.Equal(runsToAdd.Count, result.Count);
            foreach (var run in runsToAdd)
            {
                Assert.Contains(result, r => r.RunID == run.RunID && r.Distance == run.Distance);
            }
        }

        [Fact]
        public async Task AddRunsAsync_ShouldReturnEmptyList_WhenRunsIsEmpty()
        {
            var context = GetInMemoryDbContext();
            var repo = new RunRepository(context);
            var emptyRuns = new List<Run>();

            var result = await repo.AddRunsAsync(emptyRuns);

            Assert.NotNull(result);
            Assert.Empty(result);
            Assert.Equal(0, context.Run.Count());
        }
    }
}
