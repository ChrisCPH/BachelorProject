using RunningPlanner.Data;
using RunningPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace RunningPlanner.Repositories
{
    public interface IExerciseRepository
    {
        Task<Exercise> AddExerciseAsync(Exercise exercise);
        Task<Exercise?> GetExerciseByIdAsync(int exerciseId);
        Task<List<Exercise>?> GetAllExercisesByWorkoutAsync(int workoutId);
        Task<Exercise> UpdateExerciseAsync(Exercise exercise);
        Task<bool> DeleteExerciseAsync(int exerciseId);
    }

    public class ExerciseRepository : IExerciseRepository
    {
        private readonly RunningPlannerDbContext _context;

        public ExerciseRepository(RunningPlannerDbContext context)
        {
            _context = context;
        }

        public async Task<Exercise> AddExerciseAsync(Exercise exercise)
        {
            await _context.Exercise.AddAsync(exercise);
            await _context.SaveChangesAsync();
            return exercise;
        }

        public async Task<Exercise?> GetExerciseByIdAsync(int exerciseId)
        {
            return await _context.Exercise.FindAsync(exerciseId);
        }

        public async Task<List<Exercise>?> GetAllExercisesByWorkoutAsync(int workoutId)
        {
            var workout = await _context.Workout
                .Include(w => w.Exercises)
                .FirstOrDefaultAsync(w => w.WorkoutID == workoutId);

            if (workout == null) return null;

            return workout.Exercises;
        }

        public async Task<Exercise> UpdateExerciseAsync(Exercise exercise)
        {
            _context.Exercise.Update(exercise);
            await _context.SaveChangesAsync();
            return exercise;
        }

        public async Task<bool> DeleteExerciseAsync(int exerciseId)
        {
            var exercise = await _context.Exercise.FindAsync(exerciseId);
            if (exercise == null) return false;

            _context.Exercise.Remove(exercise);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}