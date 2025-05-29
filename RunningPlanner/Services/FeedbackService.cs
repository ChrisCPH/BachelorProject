using System.Net;
using RunningPlanner.Models;
using RunningPlanner.Repositories;

namespace RunningPlanner.Services
{
    public interface IFeedbackService
    {
        Task<Feedback> CreateFeedbackAsync(Feedback feedback);
        Task<Feedback?> GetFeedbackByIdAsync(int feedbackId);
        Task<Feedback?> GetFeedbackByRunAsync(int runId);
        Task<Feedback> UpdateFeedbackAsync(Feedback feedback);
        Task<bool> DeleteFeedbackAsync(int feedbackId);
    }

    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public FeedbackService(IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        public async Task<Feedback> CreateFeedbackAsync(Feedback feedback)
        {
            if (feedback == null)
            {
                throw new ArgumentNullException(nameof(feedback), "Feedback data is required.");
            }

            var existingFeedback = await _feedbackRepository.GetFeedbackByRunAsync(feedback.RunID);
            if (existingFeedback != null)
            {
                throw new InvalidOperationException("Feedback for this run already exists.");
            }

            feedback.Comment = WebUtility.HtmlEncode(feedback.Comment);

            return await _feedbackRepository.AddFeedbackAsync(feedback);
        }


        public async Task<Feedback?> GetFeedbackByIdAsync(int feedbackId)
        {
            return await _feedbackRepository.GetFeedbackByIdAsync(feedbackId);
        }

        public async Task<Feedback?> GetFeedbackByRunAsync(int runId)
        {
            return await _feedbackRepository.GetFeedbackByRunAsync(runId);
        }

        public async Task<Feedback> UpdateFeedbackAsync(Feedback feedback)
        {
            if (feedback == null)
            {
                throw new ArgumentNullException(nameof(feedback), "Feedback data is required.");
            }

            feedback.Comment = WebUtility.HtmlEncode(feedback.Comment);

            return await _feedbackRepository.UpdateFeedbackAsync(feedback);
        }

        public async Task<bool> DeleteFeedbackAsync(int feedbackId)
        {
            return await _feedbackRepository.DeleteFeedbackAsync(feedbackId);
        }
    }
}