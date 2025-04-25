using RunningPlanner.Data;
using RunningPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace RunningPlanner.Repositories
{
    public interface ITrainingPlanRepository
    {
        Task<TrainingPlan> AddTrainingPlanAsync(TrainingPlan trainingPlan, int userId);
        Task<TrainingPlan?> GetTrainingPlanByIdAsync(int trainingPlanId);
        Task<List<TrainingPlan>?> GetAllTrainingPlansByUserAsync(int userId);
        Task<TrainingPlan> UpdateTrainingPlanAsync(TrainingPlan trainingPlan);
        Task<bool> DeleteTrainingPlanAsync(int trainingPlanId);
    }

    public class TrainingPlanRepository : ITrainingPlanRepository
    {
        private readonly RunningPlannerDbContext _context;

        public TrainingPlanRepository(RunningPlannerDbContext context)
        {
            _context = context;
        }

        public async Task<TrainingPlan> AddTrainingPlanAsync(TrainingPlan trainingPlan, int userId)
        {
            await _context.TrainingPlan.AddAsync(trainingPlan);
            await _context.SaveChangesAsync();

            var userTrainingPlan = new UserTrainingPlan
            {
                UserID = userId,
                TrainingPlanID = trainingPlan.TrainingPlanID,
                Permission = "Owner"
            };

            await _context.UserTrainingPlan.AddAsync(userTrainingPlan);
            await _context.SaveChangesAsync();

            return trainingPlan;
        }


        public async Task<TrainingPlan?> GetTrainingPlanByIdAsync(int trainingPlanId)
        {
            return await _context.TrainingPlan
                .Include(tp => tp.UserTrainingPlans)
                .ThenInclude(utp => utp.User)
                .FirstOrDefaultAsync(tp => tp.TrainingPlanID == trainingPlanId);
        }

        public async Task<List<TrainingPlan>?> GetAllTrainingPlansByUserAsync(int userId)
        {
            return await _context.UserTrainingPlan
                .Where(utp => utp.UserID == userId)
                .Include(utp => utp.TrainingPlan)
                .Select(utp => utp.TrainingPlan)
                .ToListAsync();
        }

        public async Task<TrainingPlan> UpdateTrainingPlanAsync(TrainingPlan trainingPlan)
        {
            _context.TrainingPlan.Update(trainingPlan);
            await _context.SaveChangesAsync();
            return trainingPlan;
        }

        public async Task<bool> DeleteTrainingPlanAsync(int trainingPlanId)
        {
            var trainingPlan = await _context.TrainingPlan.FindAsync(trainingPlanId);
            if (trainingPlan == null) return false;

            var links = _context.UserTrainingPlan
                .Where(utp => utp.TrainingPlanID == trainingPlanId);

            _context.UserTrainingPlan.RemoveRange(links);

            _context.TrainingPlan.Remove(trainingPlan);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}