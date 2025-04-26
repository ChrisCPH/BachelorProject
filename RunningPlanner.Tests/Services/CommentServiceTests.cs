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
            var comment = new Comment { CommentID = 1 };
            _commentRepositoryMock.Setup(repo => repo.AddCommentAsync(comment)).ReturnsAsync(comment);

            var result = await _commentService.CreateCommentAsync(comment);

            Assert.Equal(comment, result);
            _commentRepositoryMock.Verify(repo => repo.AddCommentAsync(comment), Times.Once);
        }

        [Fact]
        public async Task CreateCommentAsync_ShouldThrowArgumentNullException_WhenCommentIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _commentService.CreateCommentAsync(null!));
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
            var comment = new Comment { CommentID = 1 };
            _commentRepositoryMock.Setup(repo => repo.UpdateCommentAsync(comment)).ReturnsAsync(comment);

            var result = await _commentService.UpdateCommentAsync(comment);

            Assert.Equal(comment, result);
            _commentRepositoryMock.Verify(repo => repo.UpdateCommentAsync(comment), Times.Once);
        }

        [Fact]
        public async Task UpdateCommentAsync_ShouldThrowArgumentNullException_WhenCommentIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _commentService.UpdateCommentAsync(null!));
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
