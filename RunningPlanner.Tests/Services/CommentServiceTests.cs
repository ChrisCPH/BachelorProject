using System.Net;
using Moq;
using RunningPlanner.Models;
using RunningPlanner.Repositories;
using RunningPlanner.Services;

namespace RunningPlanner.Tests.Services
{
    public class CommentServiceTests
    {
        private readonly Mock<ICommentRepository> _commentRepositoryMock;
        private readonly CommentService _commentService;

        public CommentServiceTests()
        {
            _commentRepositoryMock = new Mock<ICommentRepository>();
            _commentService = new CommentService(_commentRepositoryMock.Object);
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldReturnCreatedComment_WhenCommentIsValid()
        {
            var userId = 123;
            var comment = new Comment
            {
                CommentID = 1,
                RunID = 10,
                Text = "Test comment"
            };

            _commentRepositoryMock
                .Setup(repo => repo.AddCommentAsync(It.Is<Comment>(c => c.UserID == userId)))
                .ReturnsAsync((Comment c) => c);

            var result = await _commentService.CreateCommentAsync(comment, userId);

            Assert.Equal(userId, result.UserID);
            Assert.Equal(WebUtility.HtmlEncode(comment.Text), result.Text);
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldThrowArgumentNullException_WhenCommentIsNull()
        {
            var userId = 123;
            await Assert.ThrowsAsync<ArgumentNullException>(() => _commentService.CreateCommentAsync(null!, userId));
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldThrowArgumentException_WhenNeitherRunIdNorWorkoutIdIsSet()
        {
            var userId = 123;
            var comment = new Comment
            {
                Text = "Test",
                RunID = null,
                WorkoutID = null
            };

            await Assert.ThrowsAsync<ArgumentException>(() => _commentService.CreateCommentAsync(comment, userId));
        }

        [Fact]
        public async Task GetCommentByIdAsync_ShouldReturnComment_WhenCommentExists()
        {
            var comment = new Comment { CommentID = 1 };
            _commentRepositoryMock.Setup(repo => repo.GetCommentByIdAsync(comment.CommentID)).ReturnsAsync(comment);

            var result = await _commentService.GetCommentByIdAsync(comment.CommentID);

            Assert.Equal(comment, result);
            _commentRepositoryMock.Verify(repo => repo.GetCommentByIdAsync(comment.CommentID), Times.Once);
        }

        [Fact]
        public async Task GetAllCommentsByRunAsync_ShouldReturnComments_WhenCommentsExist()
        {
            int runId = 1;
            var comments = new List<Comment> { new Comment { CommentID = 1, RunID = runId } };
            _commentRepositoryMock.Setup(repo => repo.GetAllCommentsByRunAsync(runId)).ReturnsAsync(comments);

            var result = await _commentService.GetAllCommentsByRunAsync(runId);

            Assert.Equal(comments, result);
            _commentRepositoryMock.Verify(repo => repo.GetAllCommentsByRunAsync(runId), Times.Once);
        }

        [Fact]
        public async Task GetAllCommentsByWorkoutAsync_ShouldReturnComments_WhenCommentsExist()
        {
            int workoutId = 1;
            var comments = new List<Comment> { new Comment { CommentID = 1, WorkoutID = workoutId } };
            _commentRepositoryMock.Setup(repo => repo.GetAllCommentsByWorkoutAsync(workoutId)).ReturnsAsync(comments);

            var result = await _commentService.GetAllCommentsByWorkoutAsync(workoutId);

            Assert.Equal(comments, result);
            _commentRepositoryMock.Verify(repo => repo.GetAllCommentsByWorkoutAsync(workoutId), Times.Once);
        }

        [Fact]
        public async Task UpdateCommentAsync_ShouldReturnUpdatedComment_WhenCommentIsValid()
        {
            int userId = 123;
            var comment = new Comment
            {
                CommentID = 1,
                Text = "Updated Text",
                RunID = 10,
                WorkoutID = 5,
                CreatedAt = DateTime.UtcNow
            };

            var existingComment = new Comment
            {
                CommentID = 1,
                UserID = userId,
                Text = "Old Text",
                RunID = 10,
                WorkoutID = 5,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            _commentRepositoryMock
                .Setup(repo => repo.GetCommentByIdAsync(comment.CommentID))
                .ReturnsAsync(existingComment);

            _commentRepositoryMock
                .Setup(repo => repo.UpdateCommentAsync(It.IsAny<Comment>()))
                .ReturnsAsync((Comment c) => c);

            var result = await _commentService.UpdateCommentAsync(comment, userId);

            Assert.Equal(WebUtility.HtmlEncode(comment.Text), result.Text);
            Assert.Equal(comment.RunID, result.RunID);
            Assert.Equal(comment.WorkoutID, result.WorkoutID);
            Assert.Equal(comment.CreatedAt, result.CreatedAt);

            _commentRepositoryMock.Verify(repo => repo.GetCommentByIdAsync(comment.CommentID), Times.Once);
            _commentRepositoryMock.Verify(repo => repo.UpdateCommentAsync(It.IsAny<Comment>()), Times.Once);
        }

        [Fact]
        public async Task UpdateCommentAsync_ShouldThrowArgumentNullException_WhenCommentIsNull()
        {
            int userId = 123;
            await Assert.ThrowsAsync<ArgumentNullException>(() => _commentService.UpdateCommentAsync(null!, userId));
        }

        [Fact]
        public async Task UpdateCommentAsync_ShouldThrowKeyNotFoundException_WhenCommentDoesNotExist()
        {
            int userId = 123;
            var comment = new Comment { CommentID = 1 };

            _commentRepositoryMock
                .Setup(repo => repo.GetCommentByIdAsync(comment.CommentID))
                .ReturnsAsync((Comment?)null);

            await Assert.ThrowsAsync<KeyNotFoundException>(() => _commentService.UpdateCommentAsync(comment, userId));
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldReturnTrue_WhenCommentIsDeleted()
        {
            int commentId = 1;
            _commentRepositoryMock.Setup(repo => repo.DeleteCommentAsync(commentId)).ReturnsAsync(true);

            var result = await _commentService.DeleteCommentAsync(commentId);

            Assert.True(result);
            _commentRepositoryMock.Verify(repo => repo.DeleteCommentAsync(commentId), Times.Once);
        }
    }
}
