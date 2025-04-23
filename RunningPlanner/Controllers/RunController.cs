using Microsoft.AspNetCore.Mvc;
using RunningPlanner.Models;
using RunningPlanner.Services;

namespace RunningPlanner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RunController : ControllerBase
    {
        private readonly IRunService _runService;

        public RunController(IRunService runService)
        {
            _runService = runService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> CreateRun([FromBody] Run run)
        {
            if (run == null)
            {
                return BadRequest("Run data is required.");
            }

            var createdRun = await _runService.CreateRunAsync(run);
            return CreatedAtAction(nameof(GetRunById), new { id = createdRun.RunID }, createdRun);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRunById(int id)
        {
            var run = await _runService.GetRunByIdAsync(id);
            if (run == null)
            {
                return NotFound();
            }
            return Ok(run);
        }

        [HttpGet("trainingplan/{trainingPlanId}")]
        public async Task<IActionResult> GetAllRunsByTrainingPlan(int trainingPlanId)
        {
            var runs = await _runService.GetAllRunsByTrainingPlanAsync(trainingPlanId);
            if (runs == null || !runs.Any())
            {
                return NotFound("No runs found for the specified training plan.");
            }
            return Ok(runs);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateRun([FromBody] Run run)
        {
            if (run == null)
            {
                return BadRequest("Run data is required.");
            }

            var updatedRun = await _runService.UpdateRunAsync(run);
            return Ok(updatedRun);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRun(int id)
        {
            var result = await _runService.DeleteRunAsync(id);
            if (!result)
            {
                return NotFound("Run not found.");
            }
            return NoContent();
        }
    }
}