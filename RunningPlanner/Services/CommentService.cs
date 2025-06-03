using System.Net;
using RunningPlanner.Models;
using RunningPlanner.Repositories;

namespace RunningPlanner.Services
{
    public interface ICommentService
    {
        Task<Comment> CreateCommentAsync(Comment comment, int userId);
        Task<Comment?> GetCommentByIdAsync(int commentId);
        Task<List<Comment>?> GetAllCommentsByRunAsync(int runId);
        Task<List<Comment>?> GetAllCommentsByWorkoutAsync(int runId);
        Task<Comment> UpdateCommentAsync(Comment comment, int userId);
        Task<bool> DeleteCommentAsync(int commentId);
    }

    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<Comment> CreateCommentAsync(Comment comment, int userId)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment), "Comment data is required.");

            if (comment.RunID == null && comment.WorkoutID == null)
                throw new ArgumentException("Comment must be associated with either a Run or a Workout.");

            comment.UserID = userId;
            comment.Text = WebUtility.HtmlEncode(comment.Text);

            return await _commentRepository.AddCommentAsync(comment);
        }

        public async Task<Comment?> GetCommentByIdAsync(int commentId)
        {
            return await _commentRepository.GetCommentByIdAsync(commentId);
        }

        public async Task<List<Comment>?> GetAllCommentsByRunAsync(int runId)
        {
            return await _commentRepository.GetAllCommentsByRunAsync(runId);
        }

        public async Task<List<Comment>?> GetAllCommentsByWorkoutAsync(int workoutId)
        {
            return await _commentRepository.GetAllCommentsByWorkoutAsync(workoutId);
        }

        public async Task<Comment> UpdateCommentAsync(Comment comment, int userId)
        {
            if (comment == null)
                throw new ArgumentNullException(nameof(comment), "Comment data is required.");

            var existing = await _commentRepository.GetCommentByIdAsync(comment.CommentID);
            if (existing == null)
                throw new KeyNotFoundException("Comment not found.");

            existing.Text = WebUtility.HtmlEncode(comment.Text);
            existing.RunID = comment.RunID;
            existing.WorkoutID = comment.WorkoutID;
            existing.CreatedAt = comment.CreatedAt;

            return await _commentRepository.UpdateCommentAsync(existing);
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            return await _commentRepository.DeleteCommentAsync(commentId);
        }
    }
}