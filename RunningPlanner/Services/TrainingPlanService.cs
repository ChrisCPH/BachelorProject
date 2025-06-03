using System.Net;
using RunningPlanner.Models;
using RunningPlanner.Repositories;

namespace RunningPlanner.Services
{
    public interface ITrainingPlanService
    {
        Task<TrainingPlan> CreateTrainingPlanAsync(TrainingPlan trainingPlan, int userId);
        Task<TrainingPlan?> GetTrainingPlanByIdAsync(int trainingPlanId);
        Task<List<TrainingPlan>?> GetAllTrainingPlansByUserAsync(int userId);
        Task<TrainingPlan> UpdateTrainingPlanAsync(TrainingPlan trainingPlan);
        Task<bool> DeleteTrainingPlanAsync(int trainingPlanId);
        Task<List<TrainingPlanWithPermission>?> GetAllTrainingPlansWithPermissionsByUserAsync(int userId);
    }

    public class TrainingPlanService : ITrainingPlanService
    {
        private readonly ITrainingPlanRepository _trainingPlanRepository;
        private readonly IUserTrainingPlanRepository _userTrainingPlanRepository;

        public TrainingPlanService(ITrainingPlanRepository trainingPlanRepository, IUserTrainingPlanRepository userTrainingPlanRepository)
        {
            _trainingPlanRepository = trainingPlanRepository;
            _userTrainingPlanRepository = userTrainingPlanRepository;
        }

        public async Task<TrainingPlan> CreateTrainingPlanAsync(TrainingPlan trainingPlan, int userId)
        {
            if (trainingPlan == null)
            {
                throw new ArgumentNullException(nameof(trainingPlan), "Training plan data is required.");
            }

            trainingPlan.Name = WebUtility.HtmlEncode(trainingPlan.Name);
            trainingPlan.Event = WebUtility.HtmlEncode(trainingPlan.Event);
            trainingPlan.GoalTime = WebUtility.HtmlEncode(trainingPlan.GoalTime);

            var savedPlan = await _trainingPlanRepository.AddTrainingPlanAsync(trainingPlan);

            var userTrainingPlan = new UserTrainingPlan
            {
                UserID = userId,
                TrainingPlanID = savedPlan.TrainingPlanID,
                Permission = "owner"
            };

            await _userTrainingPlanRepository.AddUserTrainingPlanAsync(userTrainingPlan);

            return savedPlan;
        }

        public async Task<TrainingPlan?> GetTrainingPlanByIdAsync(int trainingPlanId)
        {
            return await _trainingPlanRepository.GetTrainingPlanByIdAsync(trainingPlanId);
        }

        public async Task<List<TrainingPlan>?> GetAllTrainingPlansByUserAsync(int userId)
        {
            return await _trainingPlanRepository.GetAllTrainingPlansByUserAsync(userId);
        }

        public async Task<List<TrainingPlanWithPermission>?> GetAllTrainingPlansWithPermissionsByUserAsync(int userId)
        {
            return await _trainingPlanRepository.GetAllTrainingPlansWithPermissionsByUserAsync(userId);
        }

        public async Task<TrainingPlan> UpdateTrainingPlanAsync(TrainingPlan trainingPlan)
        {
            if (trainingPlan == null)
            {
                throw new ArgumentNullException(nameof(trainingPlan), "Training plan data is required.");
            }

            trainingPlan.Name = WebUtility.HtmlEncode(trainingPlan.Name);
            trainingPlan.Event = WebUtility.HtmlEncode(trainingPlan.Event);
            trainingPlan.GoalTime = WebUtility.HtmlEncode(trainingPlan.GoalTime);

            return await _trainingPlanRepository.UpdateTrainingPlanAsync(trainingPlan);
        }

        public async Task<bool> DeleteTrainingPlanAsync(int trainingPlanId)
        {
            return await _trainingPlanRepository.DeleteTrainingPlanAsync(trainingPlanId);
        }
    }
}