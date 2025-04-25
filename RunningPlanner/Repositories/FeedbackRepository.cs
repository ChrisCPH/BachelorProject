using RunningPlanner.Data;
using RunningPlanner.Models;
using Microsoft.EntityFrameworkCore;

namespace RunningPlanner.Repositories
{
    public interface IFeedbackRepository
    {
        Task<Feedback> AddFeedbackAsync(Feedback feedback);
        Task<Feedback?> GetFeedbackByIdAsync(int feedbackId);
        Task<Feedback?> GetFeedbackByRunAsync(int runId);
        Task<Feedback> UpdateFeedbackAsync(Feedback feedback);
        Task<bool> DeleteFeedbackAsync(int feedbackId);
    }

    public class FeedbackRepository : IFeedbackRepository
    {
        private readonly RunningPlannerDbContext _context;

        public FeedbackRepository(RunningPlannerDbContext context)
        {
            _context = context;
        }

        public async Task<Feedback> AddFeedbackAsync(Feedback feedback)
        {
            await _context.Feedback.AddAsync(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<Feedback?> GetFeedbackByIdAsync(int feedbackId)
        {
            return await _context.Feedback.FindAsync(feedbackId);
        }

        public async Task<Feedback?> GetFeedbackByRunAsync(int runId)
        {
            return await _context.Feedback.FirstOrDefaultAsync(f => f.RunID == runId);
        }

        public async Task<Feedback> UpdateFeedbackAsync(Feedback feedback)
        {
            _context.Feedback.Update(feedback);
            await _context.SaveChangesAsync();
            return feedback;
        }

        public async Task<bool> DeleteFeedbackAsync(int feedbackId)
        {
            var feedback = await _context.Feedback.FindAsync(feedbackId);
            if (feedback == null) return false;

            _context.Feedback.Remove(feedback);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
