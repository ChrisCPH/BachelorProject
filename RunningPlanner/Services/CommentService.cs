using RunningPlanner.Models;
using RunningPlanner.Repositories;

namespace RunningPlanner.Services
{
    public interface ICommentService
    {
        Task<Comment> CreateCommentAsync(Comment comment);
        Task<Comment?> GetCommentByIdAsync(int commentId);
        Task<List<Comment>?> GetAllCommentsByRunAsync(int runId);
        Task<List<Comment>?> GetAllCommentsByWorkoutAsync(int runId);
        Task<Comment> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(int commentId);
    }

    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
        }

        public async Task<Comment> CreateCommentAsync(Comment comment)
        {
            if (comment == null)
            {
                throw new ArgumentNullException(nameof(comment), "Comment data is required.");
            }

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

        public async Task<Comment> UpdateCommentAsync(Comment comment)
        {
            if (comment == null)
            {
                throw new ArgumentNullException(nameof(comment), "Comment data is required.");
            }

            return await _commentRepository.UpdateCommentAsync(comment);
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            return await _commentRepository.DeleteCommentAsync(commentId);
        }
    }
}