using Microsoft.AspNetCore.Mvc;
using RunningPlanner.Models;
using RunningPlanner.Services;

namespace RunningPlanner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExerciseController : ControllerBase
    {
        private readonly IExerciseService _exerciseService;

        public ExerciseController(IExerciseService exerciseService)
        {
            _exerciseService = exerciseService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> CreateExercise([FromBody] Exercise exercise)
        {
            if (exercise == null)
            {
                return BadRequest("Exercise data is required.");
            }

            var createdExercise = await _exerciseService.CreateExerciseAsync(exercise);
            return CreatedAtAction(nameof(GetExerciseById), new { id = createdExercise.ExerciseID }, createdExercise);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetExerciseById(int id)
        {
            var exercise = await _exerciseService.GetExerciseByIdAsync(id);
            if (exercise == null)
            {
                return NotFound("Exercise not found.");
            }
            return Ok(exercise);
        }

        [HttpGet("workout/{workoutId}")]
        public async Task<IActionResult> GetAllExercisesByWorkout(int workoutId)
        {
            var exerciseList = await _exerciseService.GetAllExercisesByWorkoutAsync(workoutId);
            if (exerciseList == null || !exerciseList.Any())
            {
                return NotFound("No exercises found for the specified workout.");
            }
            return Ok(exerciseList);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateExercise([FromBody] Exercise exercise)
        {
            if (exercise == null)
            {
                return BadRequest("Exercise data is required.");
            }

            var updatedExercise = await _exerciseService.UpdateExerciseAsync(exercise);
            return Ok(updatedExercise);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteExercise(int id)
        {
            var result = await _exerciseService.DeleteExerciseAsync(id);
            if (!result)
            {
                return NotFound("Exercise not found.");
            }
            return NoContent();
        }
    }
}