using RunningPlanner.Data;
using RunningPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace RunningPlanner.Repositories
{
    public interface ICommentRepository
    {
        Task<Comment> AddCommentAsync(Comment comment);
        Task<Comment?> GetCommentByIdAsync(int commentId);
        Task<List<Comment>?> GetAllCommentsByRunAsync(int runId);
        Task<List<Comment>?> GetAllCommentsByWorkoutAsync(int runId);
        Task<Comment> UpdateCommentAsync(Comment comment);
        Task<bool> DeleteCommentAsync(int commentId);
    }

    public class CommentRepository : ICommentRepository
    {
        private readonly RunningPlannerDbContext _context;

        public CommentRepository(RunningPlannerDbContext context)
        {
            _context = context;
        }

        public async Task<Comment> AddCommentAsync(Comment comment)
        {
            await _context.Comment.AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment?> GetCommentByIdAsync(int commentId)
        {
            return await _context.Comment.FindAsync(commentId);
        }

        public async Task<List<Comment>?> GetAllCommentsByRunAsync(int runId)
        {
            var run = await _context.Run
                .Include(r => r.Comments)
                .FirstOrDefaultAsync(r => r.RunID == runId);

            if (run == null) return null;

            return run.Comments;
        }

        public async Task<List<Comment>?> GetAllCommentsByWorkoutAsync(int workoutId)
        {
            var workout = await _context.Workout
                .Include(w => w.Comments)
                .FirstOrDefaultAsync(w => w.WorkoutID == workoutId);

            if (workout == null) return null;

            return workout.Comments;
        }

        public async Task<Comment> UpdateCommentAsync(Comment comment)
        {
            _context.Comment.Update(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteCommentAsync(int commentId)
        {
            var comment = await _context.Comment.FindAsync(commentId);
            if (comment == null) return false;

            _context.Comment.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
