using RunningPlanner.Models;
using RunningPlanner.Repositories;

namespace RunningPlanner.Services
{
    public interface ITrainingPlanService
    {
        Task<TrainingPlan> CreateTrainingPlanAsync(TrainingPlan trainingPlan);
        Task<TrainingPlan?> GetTrainingPlanByIdAsync(int trainingPlanId);
        Task<List<TrainingPlan>?> GetAllTrainingPlansByUserAsync(int userId);
        Task<TrainingPlan> UpdateTrainingPlanAsync(TrainingPlan trainingPlan);
        Task<bool> DeleteTrainingPlanAsync(int trainingPlanId);
    }

    public class TrainingPlanService : ITrainingPlanService
    {
        private readonly ITrainingPlanRepository _trainingPlanRepository;

        public TrainingPlanService(ITrainingPlanRepository trainingPlanRepository)
        {
            _trainingPlanRepository = trainingPlanRepository;
        }

        public async Task<TrainingPlan> CreateTrainingPlanAsync(TrainingPlan trainingPlan)
        {
            if (trainingPlan == null)
            {
                throw new ArgumentNullException(nameof(trainingPlan), "Training plan data is required.");
            }

            return await _trainingPlanRepository.AddTrainingPlanAsync(trainingPlan);
        }

        public async Task<TrainingPlan?> GetTrainingPlanByIdAsync(int trainingPlanId)
        {
            return await _trainingPlanRepository.GetTrainingPlanByIdAsync(trainingPlanId);
        }

        public async Task<List<TrainingPlan>?> GetAllTrainingPlansByUserAsync(int userId)
        {
            return await _trainingPlanRepository.GetAllTrainingPlansByUserAsync(userId);
        }

        public async Task<TrainingPlan> UpdateTrainingPlanAsync(TrainingPlan trainingPlan)
        {
            if (trainingPlan == null)
            {
                throw new ArgumentNullException(nameof(trainingPlan), "Training plan data is required.");
            }

            return await _trainingPlanRepository.UpdateTrainingPlanAsync(trainingPlan);
        }
        
        public async Task<bool> DeleteTrainingPlanAsync(int trainingPlanId)
        {
            return await _trainingPlanRepository.DeleteTrainingPlanAsync(trainingPlanId);
        }
    }
}