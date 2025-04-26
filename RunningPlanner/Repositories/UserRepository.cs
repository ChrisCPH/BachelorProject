using RunningPlanner.Data;
using RunningPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace RunningPlanner.Repositories
{
    public interface IUserRepository
    {
        Task<User> AddUserAsync(User user);
        Task<User?> GetUserByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<bool> AddUserToTrainingPlanAsync(int userId, int trainingPlanId, string permission);

    }
    public class UserRepository : IUserRepository
    {
        private readonly RunningPlannerDbContext _context;

        public UserRepository(RunningPlannerDbContext context)
        {
            _context = context;
        }

        public async Task<User> AddUserAsync(User user)
        {
            await _context.User.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.User.FindAsync(userId);
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.User.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _context.User.FirstOrDefaultAsync(u => u.UserName == username);
        }

        public async Task<bool> AddUserToTrainingPlanAsync(int userId, int trainingPlanId, string permission)
        {
            var userExists = await _context.User.AnyAsync(u => u.UserID == userId);
            var planExists = await _context.TrainingPlan.AnyAsync(tp => tp.TrainingPlanID == trainingPlanId);

            if (!userExists || !planExists)
                return false;

            var alreadyLinked = await _context.UserTrainingPlan
                .AnyAsync(utp => utp.UserID == userId && utp.TrainingPlanID == trainingPlanId);

            if (alreadyLinked)
                return true;

            var link = new UserTrainingPlan
            {
                UserID = userId,
                TrainingPlanID = trainingPlanId,
                Permission = permission
            };

            await _context.UserTrainingPlan.AddAsync(link);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}