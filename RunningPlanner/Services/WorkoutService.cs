using RunningPlanner.Models;
using RunningPlanner.Repositories;

namespace RunningPlanner.Services
{
    public interface IWorkoutService
    {
        Task<Workout> CreateWorkoutAsync(Workout workout);
        Task<List<Workout>> CreateRepeatedWorkoutAsync(Workout workout);
        Task<Workout?> GetWorkoutByIdAsync(int workoutId);
        Task<List<Workout>?> GetAllWorkoutsByTrainingPlanAsync(int trainingPlanId);
        Task<Workout> UpdateWorkoutAsync(Workout workout);
        Task<bool> DeleteWorkoutAsync(int workoutId);
    }

    public class WorkoutService : IWorkoutService
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly ITrainingPlanRepository _trainingPlanRepository;

        public WorkoutService(IWorkoutRepository workoutRepository, ITrainingPlanRepository trainingPlanRepository)
        {
            _workoutRepository = workoutRepository;
            _trainingPlanRepository = trainingPlanRepository;
        }

        public async Task<Workout> CreateWorkoutAsync(Workout workout)
        {
            if (workout == null)
            {
                throw new ArgumentNullException(nameof(workout), "Workout data is required.");
            }

            return await _workoutRepository.AddWorkoutAsync(workout);
        }

        public async Task<List<Workout>> CreateRepeatedWorkoutAsync(Workout workout)
        {
            if (workout == null)
                throw new ArgumentNullException(nameof(workout));

            var trainingPlan = await _trainingPlanRepository.GetTrainingPlanByIdAsync(workout.TrainingPlanID);

            if (trainingPlan == null)
                throw new ArgumentException("Training plan not found");

            var workoutsToCreate = new List<Workout>();

            for (int week = 1; week <= trainingPlan.Duration; week++)
            {
                var workoutForWeek = new Workout
                {
                    TrainingPlanID = workout.TrainingPlanID,
                    Type = workout.Type,
                    WeekNumber = week,
                    DayOfWeek = workout.DayOfWeek,
                    TimeOfDay = workout.TimeOfDay,
                    Duration = workout.Duration,
                    Notes = workout.Notes,
                    Completed = workout.Completed,
                    CreatedAt = DateTime.UtcNow,
                };

                workoutsToCreate.Add(workoutForWeek);
            }

            var createdWorkouts = await _workoutRepository.AddWorkoutsAsync(workoutsToCreate);

            return createdWorkouts;
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
