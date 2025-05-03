using RunningPlanner.Models;
using RunningPlanner.Repositories;

namespace RunningPlanner.Services
{
    public interface IWorkoutService
    {
        Task<Workout> CreateWorkoutAsync(Workout workout);
        Task<Workout?> GetWorkoutByIdAsync(int workoutId);
        Task<List<Workout>?> GetAllWorkoutsByTrainingPlanAsync(int trainingPlanId);
        Task<Workout> UpdateWorkoutAsync(Workout workout);
        Task<bool> DeleteWorkoutAsync(int workoutId);
    }

    public class WorkoutService : IWorkoutService
    {
        private readonly IWorkoutRepository _workoutRepository;

        public WorkoutService(IWorkoutRepository workoutRepository)
        {
            _workoutRepository = workoutRepository;
        }

        public async Task<Workout> CreateWorkoutAsync(Workout workout)
        {
            if (workout == null)
            {
                throw new ArgumentNullException(nameof(workout), "Workout data is required.");
            }

            return await _workoutRepository.AddWorkoutAsync(workout);
        }

        public async Task<Workout?> GetWorkoutByIdAsync(int workoutId)
        {
            return await _workoutRepository.GetWorkoutByIdAsync(workoutId);
        }

        public async Task<List<Workout>?> GetAllWorkoutsByTrainingPlanAsync(int trainingPlanId)
        {
            return await _workoutRepository.GetAllWorkoutsByTrainingPlanAsync(trainingPlanId);
        }

        public async Task<Workout> UpdateWorkoutAsync(Workout workout)
        {
            if (workout == null)
            {
                throw new ArgumentNullException(nameof(workout), "Workout data is required.");
            }

            return await _workoutRepository.UpdateWorkoutAsync(workout);
        }

        public async Task<bool> DeleteWorkoutAsync(int workoutId)
        {
            return await _workoutRepository.DeleteWorkoutAsync(workoutId);
        }
    }
}
