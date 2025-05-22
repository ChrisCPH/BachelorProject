using RunningPlanner.Data;
using RunningPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace RunningPlanner.Repositories
{
    public interface IWorkoutRepository
    {
        Task<Workout> AddWorkoutAsync(Workout workout);
        Task<List<Workout>> AddWorkoutsAsync(List<Workout> workouts);
        Task<Workout?> GetWorkoutByIdAsync(int workoutId);
        Task<List<Workout>?> GetAllWorkoutsByTrainingPlanAsync(int trainingPlanId);
        Task<Workout> UpdateWorkoutAsync(Workout workout);
        Task<bool> DeleteWorkoutAsync(int workoutId);
    }

    public class WorkoutRepository : IWorkoutRepository
    {
        private readonly RunningPlannerDbContext _context;

        public WorkoutRepository(RunningPlannerDbContext context)
        {
            _context = context;
        }

        public async Task<Workout> AddWorkoutAsync(Workout workout)
        {
            await _context.Workout.AddAsync(workout);
            await _context.SaveChangesAsync();
            return workout;
        }

        public async Task<List<Workout>> AddWorkoutsAsync(List<Workout> workouts)
        {
            await _context.Workout.AddRangeAsync(workouts);
            await _context.SaveChangesAsync();
            return workouts;
        }

        public async Task<Workout?> GetWorkoutByIdAsync(int workoutId)
        {
            return await _context.Workout.FindAsync(workoutId);
        }

        public async Task<List<Workout>?> GetAllWorkoutsByTrainingPlanAsync(int trainingPlanId)
        {
            var trainingPlan = await _context.TrainingPlan
                .Include(tp => tp.Workouts)
                .FirstOrDefaultAsync(tp => tp.TrainingPlanID == trainingPlanId);

            if (trainingPlan == null) return null;

            return trainingPlan.Workouts;
            
        }

        public async Task<Workout> UpdateWorkoutAsync(Workout workout)
        {
            _context.Workout.Update(workout);
            await _context.SaveChangesAsync();
            return workout;
        }

        public async Task<bool> DeleteWorkoutAsync(int workoutId)
        {
            var workout = await _context.Workout.FindAsync(workoutId);
            if (workout == null) return false;

            _context.Workout.Remove(workout);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}