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
        Task<UserAdd?> GetUserIdByUsernameAsync(string username);
        Task<(bool Success, string Message)> AddUserToTrainingPlanAsync(int userId, int trainingPlanId, string permission);
        Task<string?> GetUserPermission(int userId, int trainingPlanId);

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

        public async Task<UserAdd?> GetUserIdByUsernameAsync(string username)
        {
            var user = await _context.User.FirstOrDefaultAsync(u => u.UserName == username);

            if (user == null)
                return null;

            var userAdd = new UserAdd
            {
                UserID = user.UserID,
                UserName = user.UserName
            };

            return userAdd;
        }


        public async Task<(bool Success, string Message)> AddUserToTrainingPlanAsync(int userId, int trainingPlanId, string permission)
        {
            var userExists = await _context.User.AnyAsync(u => u.UserID == userId);
            var planExists = await _context.TrainingPlan.AnyAsync(tp => tp.TrainingPlanID == trainingPlanId);

            if (!userExists || !planExists)
                return (false, "User or training plan not found.");

            var existingLink = await _context.UserTrainingPlan
                .FirstOrDefaultAsync(utp => utp.UserID == userId && utp.TrainingPlanID == trainingPlanId);

            if (existingLink != null)
            {
                if (existingLink.Permission.Equals(permission, StringComparison.OrdinalIgnoreCase))
                {
                    return (false, "User already has this permission.");
                }

                existingLink.Permission = permission;
                _context.UserTrainingPlan.Update(existingLink);
                await _context.SaveChangesAsync();
                return (true, "Permission updated.");
            }

            var newLink = new UserTrainingPlan
            {
                UserID = userId,
                TrainingPlanID = trainingPlanId,
                Permission = permission
            };

            await _context.UserTrainingPlan.AddAsync(newLink);
            await _context.SaveChangesAsync();
            return (true, "User added to training plan.");
        }

        public async Task<string?> GetUserPermission(int userId, int trainingPlanId)
        {
            var userTrainingPlan = await _context.UserTrainingPlan
                .Where(utp => utp.UserID == userId && utp.TrainingPlanID == trainingPlanId)
                .Select(utp => utp.Permission)
                .FirstOrDefaultAsync();

            if (userTrainingPlan == null)
            {
                return "No permission found";
            }

            return userTrainingPlan;
        }
    }
}