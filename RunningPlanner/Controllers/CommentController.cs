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
        [CommentPermissionAuthorize("commenter", "editor", "owner")]
        public async Task<IActionResult> CreateComment([FromBody] Comment comment)
        {
            if (comment == null)
            {
                return BadRequest("Comment data is required.");
            }

            var userIdClaim = User.FindFirst("UserID");
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            int userId = int.Parse(userIdClaim.Value);

            var commentWithUserId = new Comment
            {
                UserID = userId,
                RunID = comment.RunID,
                WorkoutID = comment.WorkoutID,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt
            };

            var createdComment = await _commentService.CreateCommentAsync(commentWithUserId);
            return CreatedAtAction(nameof(GetCommentById), new { id = createdComment.CommentID }, createdComment);
        }

        [HttpGet("{id}")]
        [CommentPermissionAuthorize("viewer", "commenter", "editor", "owner")]
        public async Task<IActionResult> GetCommentById(int id)
        {
            var comment = await _commentService.GetCommentByIdAsync(id);
            if (comment == null)
            {
                return NotFound("Comment not found.");
            }
            return Ok(comment);
        }

        [HttpGet("run/{id}")]
        [RunPermissionAuthorize("viewer", "commenter", "editor", "owner")]
        public async Task<IActionResult> GetAllCommentsByRun(int id)
        {
            var commentList = await _commentService.GetAllCommentsByRunAsync(id);
            if (commentList == null || !commentList.Any())
            {
                return NotFound("No comments found for the specified run.");
            }
            return Ok(commentList);
        }

        [HttpGet("workout/{id}")]
        [WorkoutPermissionAuthorize("viewer", "commenter", "editor", "owner")]
        public async Task<IActionResult> GetAllCommentsByWorkout(int id)
        {
            var commentList = await _commentService.GetAllCommentsByWorkoutAsync(id);
            if (commentList == null || !commentList.Any())
            {
                return NotFound("No comments found for the specified workout.");
            }
            return Ok(commentList);
        }

        [HttpPut("update")]
        [CommentPermissionAuthorize("editor", "owner")]
        public async Task<IActionResult> UpdateComment([FromBody] Comment comment)
        {
            if (comment == null)
            {
                return BadRequest("Comment data is required.");
            }

            var userIdClaim = User.FindFirst("UserID");
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }

            int userId = int.Parse(userIdClaim.Value);

            var commentWithUserId = new Comment
            {
                CommentID = comment.CommentID,
                UserID = userId,
                RunID = comment.RunID,
                WorkoutID = comment.WorkoutID,
                Text = comment.Text,
                CreatedAt = comment.CreatedAt
            };

            var updatedComment = await _commentService.UpdateCommentAsync(commentWithUserId);
            if (updatedComment == null)
            {
                return NotFound("Comment not found.");
            }

            return Ok(updatedComment);
        }

        [HttpDelete("delete/{id}")]
        [CommentPermissionAuthorize("owner")]
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