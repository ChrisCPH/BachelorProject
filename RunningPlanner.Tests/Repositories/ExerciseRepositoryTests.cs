using RunningPlanner.Models;
using RunningPlanner.Repositories;

namespace RunningPlanner.Tests
{
    public class ExerciseRepositoryTests : TestBase
    {
        [Fact]
        public async Task AddExerciseAsync_ShouldAddExercise()
        {
            var context = GetInMemoryDbContext();
            var repo = new ExerciseRepository(context);

            var workout = new Workout { WorkoutID = 1, Type = "Strength" };
            context.Workout.Add(workout);
            await context.SaveChangesAsync();

            var exercise = new Exercise
            {
                ExerciseID = 1,
                WorkoutID = workout.WorkoutID,
                Name = "Push-up",
                Sets = 3,
                Reps = 15
            };

            var result = await repo.AddExerciseAsync(exercise);

            Assert.NotNull(result);
            Assert.Equal(1, context.Exercise.Count());
            Assert.Equal("Push-up", result.Name);
        }

        [Fact]
        public async Task GetExerciseByIdAsync_ShouldReturnExercise()
        {
            var context = GetInMemoryDbContext();
            var repo = new ExerciseRepository(context);

            var workout = new Workout { WorkoutID = 1, Type = "Strength" };
            var exercise = new Exercise
            {
                ExerciseID = 1,
                WorkoutID = workout.WorkoutID,
                Name = "Push-up",
                Sets = 3,
                Reps = 15
            };
            context.Workout.Add(workout);
            context.Exercise.Add(exercise);
            await context.SaveChangesAsync();

            var result = await repo.GetExerciseByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Push-up", result!.Name);
        }

        [Fact]
        public async Task GetAllExercisesByWorkoutAsync_ShouldReturnExercises()
        {
            var context = GetInMemoryDbContext();
            var repo = new ExerciseRepository(context);

            var workout = new Workout { WorkoutID = 1, Type = "Strength" };
            var exercise1 = new Exercise
            {
                ExerciseID = 1,
                WorkoutID = workout.WorkoutID,
                Name = "Push-up",
                Sets = 3,
                Reps = 15
            };
            var exercise2 = new Exercise
            {
                ExerciseID = 2,
                WorkoutID = workout.WorkoutID,
                Name = "Squat",
                Sets = 4,
                Reps = 12
            };
            context.Workout.Add(workout);
            context.Exercise.AddRange(exercise1, exercise2);
            await context.SaveChangesAsync();

            var result = await repo.GetAllExercisesByWorkoutAsync(workout.WorkoutID);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Count);
            Assert.Contains(result, e => e.Name == "Push-up");
            Assert.Contains(result, e => e.Name == "Squat");
        }

        [Fact]
        public async Task UpdateExerciseAsync_ShouldUpdateExercise()
        {
            var context = GetInMemoryDbContext();
            var repo = new ExerciseRepository(context);

            var workout = new Workout { WorkoutID = 1, Type = "Strength" };
            var exercise = new Exercise
            {
                ExerciseID = 1,
                WorkoutID = workout.WorkoutID,
                Name = "Push-up",
                Sets = 3,
                Reps = 15
            };
            context.Workout.Add(workout);
            context.Exercise.Add(exercise);
            await context.SaveChangesAsync();

            exercise.Sets = 4;
            var updatedExercise = await repo.UpdateExerciseAsync(exercise);

            Assert.Equal(4, updatedExercise.Sets);
        }

        [Fact]
        public async Task DeleteExerciseAsync_ShouldRemoveExercise()
        {
            var context = GetInMemoryDbContext();
            var repo = new ExerciseRepository(context);

            var workout = new Workout { WorkoutID = 1, Type = "Strength" };
            var exercise = new Exercise
            {
                ExerciseID = 1,
                WorkoutID = workout.WorkoutID,
                Name = "Push-up",
                Sets = 3,
                Reps = 15
            };
            context.Workout.Add(workout);
            context.Exercise.Add(exercise);
            await context.SaveChangesAsync();

            var result = await repo.DeleteExerciseAsync(1);

            Assert.True(result);
            Assert.Empty(context.Exercise);
        }

        [Fact]
        public async Task DeleteExerciseAsync_ShouldReturnFalse_IfExerciseNotFound()
        {
            var context = GetInMemoryDbContext();
            var repo = new ExerciseRepository(context);

            var result = await repo.DeleteExerciseAsync(999);

            Assert.False(result);
        }
    }
}
