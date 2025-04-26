using Microsoft.AspNetCore.Mvc;
using Moq;
using RunningPlanner.Controllers;
using RunningPlanner.Models;
using RunningPlanner.Services;

namespace RunningPlanner.Tests.Controllers
{
    public class FeedbackControllerTests
    {
        private readonly Mock<IFeedbackService> _feedbackServiceMock;
        private readonly FeedbackController _controller;

        public FeedbackControllerTests()
        {
            _feedbackServiceMock = new Mock<IFeedbackService>();
            _controller = new FeedbackController(_feedbackServiceMock.Object);
        }

        [Fact]
        public async Task CreateFeedback_ReturnsCreatedAtActionResult_WhenFeedbackIsValid()
        {
            var feedback = new Feedback { FeedbackID = 1 };
            _feedbackServiceMock.Setup(s => s.CreateFeedbackAsync(feedback)).ReturnsAsync(feedback);

            var result = await _controller.CreateFeedback(feedback);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(FeedbackController.GetFeedbackById), createdResult.ActionName);
            Assert.Equal(feedback, createdResult.Value);
        }

        [Fact]
        public async Task CreateFeedback_ReturnsBadRequest_WhenFeedbackIsNull()
        {
            var result = await _controller.CreateFeedback(null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetFeedbackById_ReturnsOk_WhenFeedbackExists()
        {
            var feedback = new Feedback { FeedbackID = 1 };
            _feedbackServiceMock.Setup(s => s.GetFeedbackByIdAsync(1)).ReturnsAsync(feedback);

            var result = await _controller.GetFeedbackById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(feedback, okResult.Value);
        }

        [Fact]
        public async Task GetFeedbackById_ReturnsNotFound_WhenFeedbackDoesNotExist()
        {
            _feedbackServiceMock.Setup(s => s.GetFeedbackByIdAsync(1)).ReturnsAsync((Feedback)null!);

            var result = await _controller.GetFeedbackById(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Feedback not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetFeedbackByRun_ReturnsOk_WhenFeedbackExists()
        {
            var feedback = new Feedback { FeedbackID = 1 };
            _feedbackServiceMock.Setup(s => s.GetFeedbackByRunAsync(1)).ReturnsAsync(feedback);

            var result = await _controller.GetFeedbackByRun(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(feedback, okResult.Value);
        }

        [Fact]
        public async Task GetFeedbackByRun_ReturnsNotFound_WhenFeedbackDoesNotExist()
        {
            _feedbackServiceMock.Setup(s => s.GetFeedbackByRunAsync(1)).ReturnsAsync((Feedback)null!);

            var result = await _controller.GetFeedbackByRun(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No feedback found for the specified run.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateFeedback_ReturnsOk_WhenFeedbackIsUpdated()
        {
            var feedback = new Feedback { FeedbackID = 1 };
            _feedbackServiceMock.Setup(s => s.UpdateFeedbackAsync(feedback)).ReturnsAsync(feedback);

            var result = await _controller.UpdateFeedback(feedback);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(feedback, okResult.Value);
        }

        [Fact]
        public async Task UpdateFeedback_ReturnsBadRequest_WhenFeedbackIsNull()
        {
            var result = await _controller.UpdateFeedback(null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateFeedback_ReturnsNotFound_WhenFeedbackDoesNotExist()
        {
            var feedback = new Feedback { FeedbackID = 1 };
            _feedbackServiceMock.Setup(s => s.UpdateFeedbackAsync(feedback)).ReturnsAsync((Feedback)null!);

            var result = await _controller.UpdateFeedback(feedback);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Feedback not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteFeedback_ReturnsNoContent_WhenFeedbackIsDeleted()
        {
            _feedbackServiceMock.Setup(s => s.DeleteFeedbackAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteFeedback(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteFeedback_ReturnsNotFound_WhenFeedbackDoesNotExist()
        {
            _feedbackServiceMock.Setup(s => s.DeleteFeedbackAsync(1)).ReturnsAsync(false);

            var result = await _controller.DeleteFeedback(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Feedback not found.", notFoundResult.Value);
        }
    }
}
