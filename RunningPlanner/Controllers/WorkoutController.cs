using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RunningPlanner.Models;
using RunningPlanner.Services;

namespace RunningPlanner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkoutController : ControllerBase
    {
        private readonly IWorkoutService _workoutService;

        public WorkoutController(IWorkoutService workoutService)
        {
            _workoutService = workoutService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> CreateWorkout([FromBody] Workout workout)
        {
            if (workout == null)
            {
                return BadRequest("Workout data is required.");
            }

            var createdWorkout = await _workoutService.CreateWorkoutAsync(workout);
            return CreatedAtAction(nameof(GetWorkoutById), new { id = createdWorkout.WorkoutID }, createdWorkout);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorkoutById(int id)
        {
            var workout = await _workoutService.GetWorkoutByIdAsync(id);
            if (workout == null)
            {
                return NotFound();
            }
            return Ok(workout);
        }

        [HttpGet("run/{runId}")]
        public async Task<IActionResult> GetAllWorkoutsByTrainingPlan(int runId)
        {
            var workouts = await _workoutService.GetAllWorkoutsByTrainingPlanAsync(runId);
            if (workouts == null || !workouts.Any())
            {
                return NotFound("No workouts found for the specified run.");
            }
            return Ok(workouts);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateWorkout([FromBody] Workout workout)
        {
            if (workout == null)
            {
                return BadRequest("Workout data is required.");
            }

            var updatedWorkout = await _workoutService.UpdateWorkoutAsync(workout);
            if (updatedWorkout == null)
            {
                return NotFound("Workout not found.");
            }
            return Ok(updatedWorkout);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkout(int id)
        {
            var result = await _workoutService.DeleteWorkoutAsync(id);
            if (!result)
            {
                return NotFound("Workout not found.");
            }
            return NoContent();
        }
    }
}