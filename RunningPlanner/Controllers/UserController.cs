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
                return BadRequest(new { message = "User data is required." });
            }

            try
            {
                var createdUser = await _userService.CreateUserAsync(user);
                return CreatedAtAction(nameof(GetUserById), new { id = createdUser.UserID }, createdUser);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserNameByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpGet("get/{userName}")]
        public async Task<IActionResult> GetUserIdByName(string username)
        {
            var user = await _userService.GetUserIdByNameAsync(username);
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
                return BadRequest(new { message = "Email and password are required." });
            }

            try
            {
                var token = await _userService.LoginAsync(loginRequest.Email, loginRequest.Password);
                if (string.IsNullOrEmpty(token))
                {
                    return Unauthorized(new { message = "Invalid email or password." });
                }

                return Ok(new { message = "Login successful", token });
            }
            catch (InvalidOperationException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("addUserToTrainingPlan")]
        public async Task<IActionResult> AddUserToTrainingPlan([FromQuery] int userId, [FromQuery] int trainingPlanId, [FromQuery] string permission)
        {
            var (success, message) = await _userService.AddUserToTrainingPlanAsync(userId, trainingPlanId, permission);

            if (!success)
            {
                return BadRequest(message);
            }

            return Ok(message);
        }
    }
}