using Microsoft.AspNetCore.Mvc;
using RunningPlanner.Models;
using RunningPlanner.Services;

namespace RunningPlanner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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

            var createdTrainingPlan = await _trainingPlanService.CreateTrainingPlanAsync(trainingPlan);
            return CreatedAtAction(nameof(GetTrainingPlanById), new { id = createdTrainingPlan.TrainingPlanID }, createdTrainingPlan);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrainingPlanById(int id)
        {
            var trainingPlan = await _trainingPlanService.GetTrainingPlanByIdAsync(id);
            if (trainingPlan == null)
            {
                return NotFound();
            }
            return Ok(trainingPlan);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAllTrainingPlansByUser(int userId)
        {
            var trainingPlans = await _trainingPlanService.GetAllTrainingPlansByUserAsync(userId);
            if (trainingPlans == null || !trainingPlans.Any())
            {
                return NotFound("No training plans found for the specified user.");
            }
            return Ok(trainingPlans);
        }

        [HttpPut("update")]
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

        [HttpDelete("{id}")]
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