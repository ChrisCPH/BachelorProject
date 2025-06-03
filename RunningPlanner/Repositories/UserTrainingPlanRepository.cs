using Microsoft.EntityFrameworkCore;
using RunningPlanner.Data;
using RunningPlanner.Models;

namespace RunningPlanner.Repositories
{
    public interface IUserTrainingPlanRepository
    {
        Task<UserTrainingPlan?> GetUserTrainingPlanAsync(int userId, int trainingPlanId);
        Task<UserTrainingPlan> AddUserTrainingPlanAsync(UserTrainingPlan userTrainingPlan);
        Task<UserTrainingPlan> UpdateUserTrainingPlanAsync(UserTrainingPlan userTrainingPlan);
    }

    public class UserTrainingPlanRepository : IUserTrainingPlanRepository
    {
        private readonly RunningPlannerDbContext _context;

        public UserTrainingPlanRepository(RunningPlannerDbContext context)
        {
            _context = context;
        }

        public async Task<UserTrainingPlan?> GetUserTrainingPlanAsync(int userId, int trainingPlanId)
        {
            return await _context.UserTrainingPlan
                .FirstOrDefaultAsync(utp => utp.UserID == userId && utp.TrainingPlanID == trainingPlanId);
        }

        public async Task<UserTrainingPlan> AddUserTrainingPlanAsync(UserTrainingPlan userTrainingPlan)
        {
            await _context.UserTrainingPlan.AddAsync(userTrainingPlan);
            await _context.SaveChangesAsync();

            return userTrainingPlan;
        }

        public async Task<UserTrainingPlan> UpdateUserTrainingPlanAsync(UserTrainingPlan userTrainingPlan)
        {
            _context.UserTrainingPlan.Update(userTrainingPlan);
            await _context.SaveChangesAsync();

            return userTrainingPlan;
        }
    }
}