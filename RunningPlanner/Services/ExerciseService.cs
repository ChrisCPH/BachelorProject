using System.Net;
using RunningPlanner.Models;
using RunningPlanner.Repositories;

namespace RunningPlanner.Services
{
    public interface IExerciseService
    {
        Task<Exercise> CreateExerciseAsync(Exercise exercise);
        Task<Exercise?> GetExerciseByIdAsync(int exerciseId);
        Task<List<Exercise>?> GetAllExercisesByWorkoutAsync(int workoutId);
        Task<Exercise> UpdateExerciseAsync(Exercise exercise);
        Task<bool> DeleteExerciseAsync(int exerciseId);
    }

    public class ExerciseService : IExerciseService
    {
        private readonly IExerciseRepository _exerciseRepository;

        public ExerciseService(IExerciseRepository exerciseRepository)
        {
            _exerciseRepository = exerciseRepository;
        }

        public async Task<Exercise> CreateExerciseAsync(Exercise exercise)
        {
            if (exercise == null)
            {
                throw new ArgumentNullException(nameof(exercise), "Exercise data is required.");
            }

            exercise.Name = WebUtility.HtmlEncode(exercise.Name);

            return await _exerciseRepository.AddExerciseAsync(exercise);
        }

        public async Task<Exercise?> GetExerciseByIdAsync(int exerciseId)
        {
            return await _exerciseRepository.GetExerciseByIdAsync(exerciseId);
        }

        public async Task<List<Exercise>?> GetAllExercisesByWorkoutAsync(int workoutId)
        {
            return await _exerciseRepository.GetAllExercisesByWorkoutAsync(workoutId);
        }

        public async Task<Exercise> UpdateExerciseAsync(Exercise exercise)
        {
            if (exercise == null)
            {
                throw new ArgumentNullException(nameof(exercise), "Exercise data is required.");
            }

            exercise.Name = WebUtility.HtmlEncode(exercise.Name);

            return await _exerciseRepository.UpdateExerciseAsync(exercise);
        }

        public async Task<bool> DeleteExerciseAsync(int exerciseId)
        {
            return await _exerciseRepository.DeleteExerciseAsync(exerciseId);
        }
    }
}