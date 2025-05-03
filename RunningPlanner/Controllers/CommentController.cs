using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RunningPlanner.Models;
using RunningPlanner.Services;

namespace RunningPlanner.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService)
        {
            _commentService = commentService;
        }

        [HttpPost("add")]
        public async Task<IActionResult> CreateComment([FromBody] Comment comment)
        {
            if (comment == null)
            {
                return BadRequest("Comment data is required.");
            }

            var createdComment = await _commentService.CreateCommentAsync(comment);
            return CreatedAtAction(nameof(GetCommentById), new { id = createdComment.CommentID }, createdComment);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound("Comment not found.");
            }
            return Ok(comment);
        }

        [HttpGet("run/{runId}")]

        public async Task<IActionResult> GetAllCommentsByRun(int runId)
        {
            var commentList = await _commentService.GetAllCommentsByRunAsync(runId);
            if (commentList == null || !commentList.Any())
            {
                return NotFound("No comments found for the specified run.");
            }
            return Ok(commentList);
        }

        [HttpGet("workout/{workoutId}")]
        public async Task<IActionResult> GetAllCommentsByWorkout(int workoutId)
        {
            var commentList = await _commentService.GetAllCommentsByWorkoutAsync(workoutId);
            if (commentList == null || !commentList.Any())
            {
                return NotFound("No comments found for the specified workout.");
            }
            return Ok(commentList);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateComment([FromBody] Comment comment)
        {
            if (comment == null)
            {
                return BadRequest("Comment data is required.");
            }

            var updatedComment = await _commentService.UpdateCommentAsync(comment);
            if (updatedComment == null)
            {
                return NotFound("Comment not found.");
            }
            return Ok(updatedComment);
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var result = await _commentService.DeleteCommentAsync(id);
            if (!result)
            {
                return NotFound("Comment not found.");
            }
            return NoContent();
        }
    }
}