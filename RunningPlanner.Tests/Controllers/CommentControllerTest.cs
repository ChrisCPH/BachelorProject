using Microsoft.AspNetCore.Mvc;
using Moq;
using RunningPlanner.Controllers;
using RunningPlanner.Models;
using RunningPlanner.Services;

namespace RunningPlanner.Tests.Controllers
{
    public class CommentControllerTests
    {
        private readonly Mock<ICommentService> _commentServiceMock;
        private readonly CommentController _controller;

        public CommentControllerTests()
        {
            _commentServiceMock = new Mock<ICommentService>();
            _controller = new CommentController(_commentServiceMock.Object);
        }

        [Fact]
        public async Task CreateComment_ReturnsCreatedAtAction_WhenCommentIsValid()
        {
            var comment = new Comment { CommentID = 1 };
            _commentServiceMock.Setup(s => s.CreateCommentAsync(comment)).ReturnsAsync(comment);

            var result = await _controller.CreateComment(comment);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(CommentController.GetCommentById), createdResult.ActionName);
            Assert.Equal(comment, createdResult.Value);
        }

        [Fact]
        public async Task CreateComment_ReturnsBadRequest_WhenCommentIsNull()
        {
            var result = await _controller.CreateComment(null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetCommentById_ReturnsOk_WhenCommentExists()
        {
            var comment = new Comment { CommentID = 1 };
            _commentServiceMock.Setup(s => s.GetCommentByIdAsync(1)).ReturnsAsync(comment);

            var result = await _controller.GetCommentById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(comment, okResult.Value);
        }

        [Fact]
        public async Task GetCommentById_ReturnsNotFound_WhenCommentDoesNotExist()
        {
            _commentServiceMock.Setup(s => s.GetCommentByIdAsync(1)).ReturnsAsync((Comment)null!);

            var result = await _controller.GetCommentById(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Comment not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetAllCommentsByRun_ReturnsOk_WhenCommentsExist()
        {
            var comments = new List<Comment> { new Comment { CommentID = 1 } };
            _commentServiceMock.Setup(s => s.GetAllCommentsByRunAsync(1)).ReturnsAsync(comments);

            var result = await _controller.GetAllCommentsByRun(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(comments, okResult.Value);
        }

        [Fact]
        public async Task GetAllCommentsByRun_ReturnsNotFound_WhenNoCommentsFound()
        {
            _commentServiceMock.Setup(s => s.GetAllCommentsByRunAsync(1)).ReturnsAsync(new List<Comment>());

            var result = await _controller.GetAllCommentsByRun(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No comments found for the specified run.", notFoundResult.Value);
        }

        [Fact]
        public async Task GetAllCommentsByWorkout_ReturnsOk_WhenCommentsExist()
        {
            var comments = new List<Comment> { new Comment { CommentID = 1 } };
            _commentServiceMock.Setup(s => s.GetAllCommentsByWorkoutAsync(1)).ReturnsAsync(comments);

            var result = await _controller.GetAllCommentsByWorkout(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(comments, okResult.Value);
        }

        [Fact]
        public async Task GetAllCommentsByWorkout_ReturnsNotFound_WhenNoCommentsFound()
        {
            _commentServiceMock.Setup(s => s.GetAllCommentsByWorkoutAsync(1)).ReturnsAsync(new List<Comment>());

            var result = await _controller.GetAllCommentsByWorkout(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("No comments found for the specified workout.", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateComment_ReturnsOk_WhenCommentIsUpdated()
        {
            var comment = new Comment { CommentID = 1 };
            _commentServiceMock.Setup(s => s.UpdateCommentAsync(comment)).ReturnsAsync(comment);

            var result = await _controller.UpdateComment(comment);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(comment, okResult.Value);
        }

        [Fact]
        public async Task UpdateComment_ReturnsBadRequest_WhenCommentIsNull()
        {
            var result = await _controller.UpdateComment(null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateComment_ReturnsNotFound_WhenCommentDoesNotExist()
        {
            var comment = new Comment { CommentID = 1 };
            _commentServiceMock.Setup(s => s.UpdateCommentAsync(comment)).ReturnsAsync((Comment)null!);

            var result = await _controller.UpdateComment(comment);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Comment not found.", notFoundResult.Value);
        }

        [Fact]
        public async Task DeleteComment_ReturnsNoContent_WhenCommentIsDeleted()
        {
            _commentServiceMock.Setup(s => s.DeleteCommentAsync(1)).ReturnsAsync(true);

            var result = await _controller.DeleteComment(1);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteComment_ReturnsNotFound_WhenCommentDoesNotExist()
        {
            _commentServiceMock.Setup(s => s.DeleteCommentAsync(1)).ReturnsAsync(false);

            var result = await _controller.DeleteComment(1);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Comment not found.", notFoundResult.Value);
        }
    }
}
