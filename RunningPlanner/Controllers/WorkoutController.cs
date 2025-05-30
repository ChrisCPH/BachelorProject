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
        [TrainingPlanPermissionAuthorize("editor", "owner")]
        public async Task<IActionResult> CreateWorkout([FromBody] Workout workout)
        {
            if (workout == null)
            {
                return BadRequest("Workout data is required.");
            }

            if (workout.DayOfWeek == null || workout.Type == "" || workout.WeekNumber == 0)
            {
                return BadRequest("Day of week, type, and week number are required.");
            }

            var createdWorkout = await _workoutService.CreateWorkoutAsync(workout);
            return CreatedAtAction(nameof(GetWorkoutById), new { id = createdWorkout.WorkoutID }, createdWorkout);
        }

        [HttpPost("add/repeat")]
        [TrainingPlanPermissionAuthorize("editor", "owner")]
        public async Task<IActionResult> CreateRepeatedWorkout([FromBody] Workout workout)
        {
            if (workout == null)
            {
                return BadRequest("Workout data is required.");
            }

            if (workout.DayOfWeek == null || workout.Type == "")
            {
                return BadRequest("Day of week and week number are required.");
            }

            try
            {
                var createdWorkouts = await _workoutService.CreateRepeatedWorkoutAsync(workout);
                return CreatedAtAction(nameof(GetWorkoutById), new { id = createdWorkouts.First().WorkoutID }, createdWorkouts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [WorkoutPermissionAuthorize("viewer", "commenter", "editor", "owner")]
        public async Task<IActionResult> GetWorkoutById(int id)
        {
            var workout = await _workoutService.GetWorkoutByIdAsync(id);
            if (workout == null)
            {
                return NotFound();
            }
            return Ok(workout);
        }

        [HttpGet("trainingPlan/{id}")]
        [TrainingPlanPermissionAuthorize("viewer", "commenter", "editor", "owner")]
        public async Task<IActionResult> GetAllWorkoutsByTrainingPlan(int id)
        {
            var workouts = await _workoutService.GetAllWorkoutsByTrainingPlanAsync(id);
            if (workouts == null || !workouts.Any())
            {
                return NotFound("No workouts found for the specified workout.");
            }
            return Ok(workouts);
        }

        [HttpPut("update")]
        [WorkoutPermissionAuthorize("editor", "owner")]
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

        [HttpDelete("delete/{id}")]
        [WorkoutPermissionAuthorize("owner")]
        public async Task<IActionResult> DeleteWorkout(int id)
        {
            var result = await _workoutService.DeleteWorkoutAsync(id);
            if (!result)
            {
                return NotFound("Workout not found.");
            }
            return NoContent();
        }

        [HttpPatch("complete/{id}")]
        [WorkoutPermissionAuthorize("owner")]
        public async Task<IActionResult> UpdateWorkoutCompletedStatus(int id, [FromBody] bool completed)
        {
            var workout = await _workoutService.GetWorkoutByIdAsync(id);
            if (workout == null)
            {
                return NotFound("Workout not found.");
            }

            workout.Completed = completed;
            var updatedWorkout = await _workoutService.UpdateWorkoutAsync(workout);

            return Ok(updatedWorkout);
        }
    }
}