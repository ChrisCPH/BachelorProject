using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RunningPlanner.Models;
using RunningPlanner.Services;

namespace RunningPlanner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;

        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> CreateFeedback([FromBody] Feedback feedback)
        {
            if (feedback == null)
            {
                return BadRequest("Feedback data is required.");
            }

            var createdFeedback = await _feedbackService.CreateFeedbackAsync(feedback);
            return CreatedAtAction(nameof(GetFeedbackById), new { id = createdFeedback.FeedbackID }, createdFeedback);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeedbackById(int id)
        {
            var feedback = await _feedbackService.GetFeedbackByIdAsync(id);
            if (feedback == null)
            {
                return NotFound("Feedback not found.");
            }
            return Ok(feedback);
        }

        [HttpGet("run/{runId}")]
        public async Task<IActionResult> GetFeedbackByRun(int runId)
        {
            var feedback = await _feedbackService.GetFeedbackByRunAsync(runId);
            if (feedback == null)
            {
                return NotFound("No feedback found for the specified run.");
            }
            return Ok(feedback);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateFeedback([FromBody] Feedback feedback)
        {
            if (feedback == null)
            {
                return BadRequest("Feedback data is required.");
            }

            var updatedFeedback = await _feedbackService.UpdateFeedbackAsync(feedback);
            if (updatedFeedback == null)
            {
                return NotFound("Feedback not found.");
            }
            return Ok(updatedFeedback);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            var result = await _feedbackService.DeleteFeedbackAsync(id);
            if (!result)
            {
                return NotFound("Feedback not found.");
            }
            return NoContent();
        }
    }
}