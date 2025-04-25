using RunningPlanner.Models;
using RunningPlanner.Repositories;
namespace RunningPlanner.Tests
{
    public class WorkoutRepositoryTests : TestBase
    {
        [Fact]
        public async Task AddWorkoutAsync_ShouldAddWorkout()
        {
            var context = GetInMemoryDbContext();
            var repo = new WorkoutRepository(context);

            var trainingPlan = new TrainingPlan { TrainingPlanID = 1, Name = "Strength Training Plan" };
            context.TrainingPlan.Add(trainingPlan);
            await context.SaveChangesAsync();

            var workout = new Workout
            {
                WorkoutID = 1,
                TrainingPlanID = trainingPlan.TrainingPlanID,
                Type = "Strength"
            };

            var result = await repo.AddWorkoutAsync(workout);

            Assert.NotNull(result);
            Assert.Equal(1, context.Workout.Count());
            Assert.Equal("Strength", result.Type);
        }

        [Fact]
        public async Task GetWorkoutByIdAsync_ShouldReturnWorkout()
        {
            var context = GetInMemoryDbContext();
            var repo = new WorkoutRepository(context);

            var trainingPlan = new TrainingPlan { TrainingPlanID = 1, Name = "Endurance Plan" };
            var workout = new Workout
            {
                WorkoutID = 1,
                TrainingPlanID = trainingPlan.TrainingPlanID,
                Type = "Leg Day"
            };
            context.TrainingPlan.Add(trainingPlan);
            context.Workout.Add(workout);
            await context.SaveChangesAsync();

            var result = await repo.GetWorkoutByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Leg Day", result!.Type);
        }

        [Fact]
        public async Task GetAllWorkoutsByTrainingPlanAsync_ShouldReturnListOfWorkouts()
        {
            var context = GetInMemoryDbContext();
            var repo = new WorkoutRepository(context);

            var trainingPlan = new TrainingPlan { TrainingPlanID = 1, Name = "Full Marathon" };
            var workout1 = new Workout
            {
                WorkoutID = 1,
                TrainingPlanID = trainingPlan.TrainingPlanID,
                Type = "Strength"
            };
            var workout2 = new Workout
            {
                WorkoutID = 2,
                TrainingPlanID = trainingPlan.TrainingPlanID,
                Type = "Leg Day"
            };
            context.TrainingPlan.Add(trainingPlan);
            context.Workout.AddRange(workout1, workout2);
            await context.SaveChangesAsync();

            var result = await repo.GetAllWorkoutsByTrainingPlanAsync(trainingPlan.TrainingPlanID);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Count);
            Assert.Contains(result, w => w.Type == "Strength");
            Assert.Contains(result, w => w.Type == "Leg Day");
        }

        [Fact]
        public async Task UpdateWorkoutAsync_ShouldUpdateWorkout()
        {
            var context = GetInMemoryDbContext();
            var repo = new WorkoutRepository(context);

            var trainingPlan = new TrainingPlan { TrainingPlanID = 1, Name = "Beginner Plan" };
            var workout = new Workout
            {
                WorkoutID = 1,
                TrainingPlanID = trainingPlan.TrainingPlanID,
                Type = "Strength"
            };
            context.TrainingPlan.Add(trainingPlan);
            context.Workout.Add(workout);
            await context.SaveChangesAsync();

            workout.Type = "Leg Day";
            var updatedWorkout = await repo.UpdateWorkoutAsync(workout);

            Assert.Equal("Leg Day", updatedWorkout.Type);
        }

        [Fact]
        public async Task DeleteWorkoutAsync_ShouldRemoveWorkout()
        {
            var context = GetInMemoryDbContext();
            var repo = new WorkoutRepository(context);

            var trainingPlan = new TrainingPlan { TrainingPlanID = 1, Name = "Strength Plan" };
            var workout = new Workout
            {
                WorkoutID = 1,
                TrainingPlanID = trainingPlan.TrainingPlanID,
                Type = "Strength"
            };
            context.TrainingPlan.Add(trainingPlan);
            context.Workout.Add(workout);
            await context.SaveChangesAsync();

            var result = await repo.DeleteWorkoutAsync(1);

            Assert.True(result);
            Assert.Empty(context.Workout);
        }

        [Fact]
        public async Task DeleteWorkoutAsync_ShouldReturnFalse_IfWorkoutNotFound()
        {
            var context = GetInMemoryDbContext();
            var repo = new WorkoutRepository(context);

            var result = await repo.DeleteWorkoutAsync(999);

            Assert.False(result);
        }
    }
}
