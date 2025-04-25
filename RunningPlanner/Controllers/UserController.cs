using Microsoft.AspNetCore.Mvc;
using RunningPlanner.Models;
using RunningPlanner.Services;

namespace RunningPlanner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("User data is required.");
            }

            var createdUser = await _userService.CreateUserAsync(user);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.UserID }, createdUser);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest("Email and password are required.");
            }

            var token = await _userService.LoginAsync(loginRequest.Email, loginRequest.Password);
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Invalid email or password.");
            }

            return Ok(new { Message = "Login successful", Token = token });
        }

        [HttpPost("addUserToTrainingPlan")]
        public async Task<IActionResult> AddUserToTrainingPlan([FromQuery] int userId, [FromQuery] int trainingPlanId, [FromQuery] string permission)
        {
            var success = await _userService.AddUserToTrainingPlanAsync(userId, trainingPlanId, permission);
            if (!success)
            {
                return BadRequest("Failed to follow training plan.");
            }

            return Ok("User is now following the training plan");
        }
    }
}