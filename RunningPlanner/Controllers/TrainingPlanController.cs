using Microsoft.AspNetCore.Mvc;
using RunningPlanner.Models;
using RunningPlanner.Services;
using Microsoft.AspNetCore.Authorization;

namespace RunningPlanner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TrainingPlanController : ControllerBase
    {
        private readonly ITrainingPlanService _trainingPlanService;

        public TrainingPlanController(ITrainingPlanService trainingPlanService)
        {
            _trainingPlanService = trainingPlanService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> CreateTrainingPlan([FromBody] TrainingPlan trainingPlan)
        {
            if (trainingPlan == null)
            {
                return BadRequest("Training plan data is required.");
            }

            if (trainingPlan.Name == "")
            {
                return BadRequest("Training plan name is required.");
            }

            var userIdClaim = User.FindFirst("UserID");
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            int userId = int.Parse(userIdClaim.Value);

            var createdTrainingPlan = await _trainingPlanService.CreateTrainingPlanAsync(trainingPlan, userId);
            return CreatedAtAction(nameof(GetTrainingPlanById), new { id = createdTrainingPlan.TrainingPlanID }, createdTrainingPlan);
        }

        [HttpGet("{id}")]
        [TrainingPlanPermissionAuthorize ("viewer", "commenter", "editor", "owner")]
        public async Task<IActionResult> GetTrainingPlanById(int id)
        {
            var trainingPlan = await _trainingPlanService.GetTrainingPlanByIdAsync(id);
            if (trainingPlan == null)
            {
                return NotFound();
            }
            return Ok(trainingPlan);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetAllTrainingPlansByUser()
        {
            var userIdClaim = User.FindFirst("UserID");
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            int userId = int.Parse(userIdClaim.Value);

            var trainingPlans = await _trainingPlanService.GetAllTrainingPlansByUserAsync(userId);
            if (trainingPlans == null || !trainingPlans.Any())
            {
                return NotFound("No training plans found for the user.");
            }
            return Ok(trainingPlans);
        }

        [HttpGet("planswithpermission")]
        public async Task<IActionResult> GetAllTrainingPlansByUserWithPermissions()
        {
            var userIdClaim = User.FindFirst("UserID");
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            int userId = int.Parse(userIdClaim.Value);

            var trainingPlans = await _trainingPlanService.GetAllTrainingPlansWithPermissionsByUserAsync(userId);
            if (trainingPlans == null || !trainingPlans.Any())
            {
                return NotFound("No training plans found for the user.");
            }
            return Ok(trainingPlans);
        }

        [HttpPut("update")]
        [TrainingPlanPermissionAuthorize ("owner", "editor")]
        public async Task<IActionResult> UpdateTrainingPlan([FromBody] TrainingPlan trainingPlan)
        {
            if (trainingPlan == null)
            {
                return BadRequest("Training plan data is required.");
            }

            var updatedTrainingPlan = await _trainingPlanService.UpdateTrainingPlanAsync(trainingPlan);
            if (updatedTrainingPlan == null)
            {
                return NotFound("Training plan not found.");
            }
            return Ok(updatedTrainingPlan);
        }

        [HttpDelete("delete/{id}")]
        [TrainingPlanPermissionAuthorize ("owner")]
        public async Task<IActionResult> DeleteTrainingPlan(int id)
        {
            var result = await _trainingPlanService.DeleteTrainingPlanAsync(id);
            if (!result)
            {
                return NotFound("Training plan not found.");
            }
            return NoContent();
        }
    }
}