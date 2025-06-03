using RunningPlanner.Data;
using RunningPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace RunningPlanner.Repositories
{
    public interface IUserRepository
    {
        Task<User> AddUserAsync(User user);
        Task<User?> GetUserByIdAsync(int userId);
        Task<UserAdd?> GetUserNameByIdAsync(int userId);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByUsernameAsync(string username);
        Task<UserAdd?> GetUserIdByUsernameAsync(string username);
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

        public async Task<UserAdd?> GetUserNameByIdAsync(int userId)
        {
            var user = await _context.User.FindAsync(userId);

            if (user == null)
                return null;

            var userAdd = new UserAdd
            {
                UserID = user.UserID,
                UserName = user.UserName
            };

            return userAdd;
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
    }
}