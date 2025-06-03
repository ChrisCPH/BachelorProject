using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
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
            var inputComment = new Comment
            {
                RunID = 10,
                WorkoutID = 5,
                Text = "Great run!",
                CreatedAt = DateTime.UtcNow
            };

            var expectedUserId = 123;

            var createdComment = new Comment
            {
                CommentID = 1,
                UserID = expectedUserId,
                RunID = 10,
                WorkoutID = 5,
                Text = WebUtility.HtmlEncode("Great run!"),
                CreatedAt = inputComment.CreatedAt
            };

            _commentServiceMock.Setup(s => s.CreateCommentAsync(It.IsAny<Comment>(), expectedUserId))
                               .ReturnsAsync(createdComment);

            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("UserID", expectedUserId.ToString())
            ], "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var result = await _controller.CreateComment(inputComment);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(CommentController.GetCommentById), createdResult.ActionName);
            Assert.Equal(createdComment, createdResult.Value);
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
            var inputComment = new Comment
            {
                CommentID = 1,
                RunID = 10,
                WorkoutID = 5,
                Text = "Updated text",
                CreatedAt = DateTime.UtcNow
            };

            int expectedUserId = 123;

            var updatedComment = new Comment
            {
                CommentID = 1,
                UserID = expectedUserId,
                RunID = 10,
                WorkoutID = 5,
                Text = "Updated text",
                CreatedAt = inputComment.CreatedAt
            };

            _commentServiceMock
                .Setup(s => s.UpdateCommentAsync(It.IsAny<Comment>(), expectedUserId))
                .ReturnsAsync(updatedComment);

            var user = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim("UserID", expectedUserId.ToString())
            ], "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };

            var result = await _controller.UpdateComment(inputComment);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(updatedComment, okResult.Value);
        }

        [Fact]
        public async Task UpdateComment_ReturnsBadRequest_WhenCommentIsNull()
        {
            var result = await _controller.UpdateComment(null!);

            Assert.IsType<BadRequestObjectResult>(result);
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
