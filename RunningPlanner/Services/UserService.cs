using RunningPlanner.Models;
using RunningPlanner.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Net;

namespace RunningPlanner.Services
{
    public interface IUserService
    {
        Task<User> CreateUserAsync(User user);
        Task<UserAdd?> GetUserNameByIdAsync(int userId);
        Task<UserAdd?> GetUserIdByNameAsync(string username);
        Task<string> LoginAsync(string email, string password);
        Task<(bool Success, string Message)> AddUserToTrainingPlanAsync(int userId, int trainingPlanId, string permission);
    }

    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public UserService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _passwordHasher = new PasswordHasher<User>();
            _configuration = configuration;
        }

        public async Task<User> CreateUserAsync(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User data is required.");
            }

            user.UserName = WebUtility.HtmlEncode(user.UserName);
            user.Email = WebUtility.HtmlEncode(user.Email);

            if (string.IsNullOrEmpty(user.Password))
            {
                throw new ArgumentException("Password is required.");
            }

            if (user.Password.Length < 8 || !user.Password.Any(char.IsDigit))
            {
                throw new ArgumentException("Password must be at least 8 characters long and contain at least one number.");
            }

            var existingUser = await _userRepository.GetUserByEmailAsync(user.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"A user with the email '{user.Email}' already exists.");
            }

            var existingUsername = await _userRepository.GetUserByUsernameAsync(user.UserName);
            if (existingUsername != null)
            {
                throw new InvalidOperationException($"A user with the username '{user.UserName}' already exists.");
            }

            user.Password = _passwordHasher.HashPassword(user, user.Password);

            user.CreatedAt = DateTime.UtcNow;

            return await _userRepository.AddUserAsync(user);
        }

        public async Task<UserAdd?> GetUserNameByIdAsync(int userId)
        {
            return await _userRepository.GetUserNameByIdAsync(userId);
        }

        public async Task<string> LoginAsync(string email, string password)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                throw new InvalidOperationException("User with the specified email does not exist.");
            }

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, password);
            if (result != PasswordVerificationResult.Success)
            {
                throw new InvalidOperationException("Incorrect password.");
            }

            return GenerateJwtToken(user);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT key is missing."));
            var credentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserID", user.UserID.ToString()),
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpiresInMinutes"] ?? throw new InvalidOperationException("JWT timer is missing."))),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<(bool Success, string Message)> AddUserToTrainingPlanAsync(int userId, int trainingPlanId, string permission)
        {
            var validPermissions = new[] { "editor", "commenter", "viewer" };
            if (!validPermissions.Contains(permission.ToLower()))
            {
                return (false, "Invalid permission. Must be one of: editor, commenter, viewer.");
            }

            var existingUserTrainingPlan = await _userRepository.GetUserPermission(userId, trainingPlanId);
            if (existingUserTrainingPlan != null && existingUserTrainingPlan == "owner")
            {
                return (false, "User already has 'Owner' permission for this training plan.");
            }

            permission = WebUtility.HtmlEncode(permission);

            return await _userRepository.AddUserToTrainingPlanAsync(userId, trainingPlanId, permission.ToLower());
        }


        public async Task<UserAdd?> GetUserIdByNameAsync(string username)
        {
            return await _userRepository.GetUserIdByUsernameAsync(username);
        }
    }
}