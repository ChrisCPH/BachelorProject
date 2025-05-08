using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RunningPlanner.Models;
using RunningPlanner.Services;

namespace RunningPlanner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RunController : ControllerBase
    {
        private readonly IRunService _runService;

        public RunController(IRunService runService)
        {
            _runService = runService;
        }

        [HttpPost("add")]
        [TrainingPlanPermissionAuthorize ("editor", "owner")]
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
        [RunPermissionAuthorize("viewer", "commenter", "editor", "owner")]
        public async Task<IActionResult> GetRunById(int id)
        {
            var run = await _runService.GetRunByIdAsync(id);
            if (run == null)
            {
                return NotFound();
            }
            return Ok(run);
        }

        [HttpGet("trainingplan/{id}")]
        [TrainingPlanPermissionAuthorize ("viewer", "commenter", "editor", "owner")]
        public async Task<IActionResult> GetAllRunsByTrainingPlan(int id)
        {
            var runs = await _runService.GetAllRunsByTrainingPlanAsync(id);
            if (runs == null || !runs.Any())
            {
                return NotFound("No runs found for the specified training plan.");
            }
            return Ok(runs);
        }

        [HttpPut("update")]
        [RunPermissionAuthorize("owner", "editor")]
        public async Task<IActionResult> UpdateRun([FromBody] Run run)
        {
            if (run == null)
            {
                return BadRequest("Run data is required.");
            }

            var updatedRun = await _runService.UpdateRunAsync(run);
            return Ok(updatedRun);
        }

        [HttpDelete("delete/{id}")]
        [RunPermissionAuthorize("owner")]
        public async Task<IActionResult> DeleteRun(int id)
        {
            var result = await _runService.DeleteRunAsync(id);
            if (!result)
            {
                return NotFound("Run not found.");
            }
            return NoContent();
        }

        [HttpPatch("complete/{id}")]
        [RunPermissionAuthorize("owner")]
        public async Task<IActionResult> UpdateRunCompletedStatus(int id, [FromBody] bool completed)
        {
            var run = await _runService.GetRunByIdAsync(id);
            if (run == null)
            {
                return NotFound("Run not found.");
            }

            run.Completed = completed;
            var updatedRun = await _runService.UpdateRunAsync(run);

            return Ok(updatedRun);
        }
    }
}