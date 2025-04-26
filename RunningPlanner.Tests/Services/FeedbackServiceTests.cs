using Moq;
using RunningPlanner.Models;
using RunningPlanner.Repositories;
using RunningPlanner.Services;

namespace RunningPlanner.Tests.Services
{
    public class FeedbackServiceTests
    {
        private readonly Mock<IFeedbackRepository> _feedbackRepositoryMock;
        private readonly FeedbackService _feedbackService;

        public FeedbackServiceTests()
        {
            _feedbackRepositoryMock = new Mock<IFeedbackRepository>();
            _feedbackService = new FeedbackService(_feedbackRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateFeedbackAsync_ShouldReturnCreatedFeedback_WhenFeedbackIsValid()
        {
            var feedback = new Feedback { FeedbackID = 1 };
            _feedbackRepositoryMock.Setup(repo => repo.AddFeedbackAsync(feedback)).ReturnsAsync(feedback);

            var result = await _feedbackService.CreateFeedbackAsync(feedback);

            Assert.Equal(feedback, result);
            _feedbackRepositoryMock.Verify(repo => repo.AddFeedbackAsync(feedback), Times.Once);
        }

        [Fact]
        public async Task CreateFeedbackAsync_ShouldThrowArgumentNullException_WhenFeedbackIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _feedbackService.CreateFeedbackAsync(null!));
        }

        [Fact]
        public async Task GetFeedbackByIdAsync_ShouldReturnFeedback_WhenFeedbackExists()
        {
            var feedback = new Feedback { FeedbackID = 1 };
            _feedbackRepositoryMock.Setup(repo => repo.GetFeedbackByIdAsync(feedback.FeedbackID)).ReturnsAsync(feedback);

            var result = await _feedbackService.GetFeedbackByIdAsync(feedback.FeedbackID);

            Assert.Equal(feedback, result);
            _feedbackRepositoryMock.Verify(repo => repo.GetFeedbackByIdAsync(feedback.FeedbackID), Times.Once);
        }

        [Fact]
        public async Task GetFeedbackByRunAsync_ShouldReturnFeedback_WhenFeedbackExistsForRun()
        {
            var runId = 1;
            var feedback = new Feedback { FeedbackID = 1, RunID = runId };
            _feedbackRepositoryMock.Setup(repo => repo.GetFeedbackByRunAsync(runId)).ReturnsAsync(feedback);

            var result = await _feedbackService.GetFeedbackByRunAsync(runId);

            Assert.Equal(feedback, result);
            _feedbackRepositoryMock.Verify(repo => repo.GetFeedbackByRunAsync(runId), Times.Once);
        }

        [Fact]
        public async Task UpdateFeedbackAsync_ShouldReturnUpdatedFeedback_WhenFeedbackIsValid()
        {
            var feedback = new Feedback { FeedbackID = 1 };
            _feedbackRepositoryMock.Setup(repo => repo.UpdateFeedbackAsync(feedback)).ReturnsAsync(feedback);

            var result = await _feedbackService.UpdateFeedbackAsync(feedback);

            Assert.Equal(feedback, result);
            _feedbackRepositoryMock.Verify(repo => repo.UpdateFeedbackAsync(feedback), Times.Once);
        }

        [Fact]
        public async Task UpdateFeedbackAsync_ShouldThrowArgumentNullException_WhenFeedbackIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _feedbackService.UpdateFeedbackAsync(null!));
        }

        [Fact]
        public async Task DeleteFeedbackAsync_ShouldReturnTrue_WhenFeedbackIsDeleted()
        {
            int feedbackId = 1;
            _feedbackRepositoryMock.Setup(repo => repo.DeleteFeedbackAsync(feedbackId)).ReturnsAsync(true);

            var result = await _feedbackService.DeleteFeedbackAsync(feedbackId);

            Assert.True(result);
            _feedbackRepositoryMock.Verify(repo => repo.DeleteFeedbackAsync(feedbackId), Times.Once);
        }
    }
}
