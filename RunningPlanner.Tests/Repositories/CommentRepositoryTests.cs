using RunningPlanner.Models;
using RunningPlanner.Repositories;

namespace RunningPlanner.Tests
{
    public class CommentRepositoryTests : TestBase
    {
        [Fact]
        public async Task AddCommentAsync_ShouldAddComment()
        {
            var context = GetInMemoryDbContext();
            var repo = new CommentRepository(context);

            var user = new User { UserID = 1, UserName = "test" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var comment = new Comment
            {
                CommentID = 1,
                UserID = user.UserID,
                Text = "Great run!"
            };

            var result = await repo.AddCommentAsync(comment);

            Assert.NotNull(result);
            Assert.Equal(1, context.Comment.Count());
            Assert.Equal("Great run!", result.Text);
        }

        [Fact]
        public async Task GetCommentByIdAsync_ShouldReturnComment()
        {
            var context = GetInMemoryDbContext();
            var repo = new CommentRepository(context);

            var user = new User { UserID = 1, UserName = "test" };
            context.User.Add(user);
            await context.SaveChangesAsync();
            var comment = new Comment
            {
                CommentID = 1,
                UserID = user.UserID,
                Text = "Great run!"
            };
            context.Comment.Add(comment);
            await context.SaveChangesAsync();

            var result = await repo.GetCommentByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Great run!", result!.Text);
        }

        [Fact]
        public async Task GetAllCommentsByRunAsync_ShouldReturnComments()
        {
            var context = GetInMemoryDbContext();
            var repo = new CommentRepository(context);

            var user = new User { UserID = 1, UserName = "test" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var run = new Run { RunID = 1, Distance = 5.0 };

            var comment1 = new Comment
            {
                CommentID = 1,
                UserID = user.UserID,
                RunID = run.RunID,
                Text = "Great run!"
            };
            var comment2 = new Comment
            {
                CommentID = 2,
                UserID = user.UserID,
                RunID = run.RunID,
                Text = "Well done!"
            };
            context.Run.Add(run);
            context.Comment.AddRange(comment1, comment2);
            await context.SaveChangesAsync();

            var result = await repo.GetAllCommentsByRunAsync(user.UserID);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Count);
            Assert.Contains(result, c => c.Text == "Great run!");
            Assert.Contains(result, c => c.Text == "Well done!");
        }

        [Fact]
        public async Task GetAllCommentsByWorkoutAsync_ShouldReturnComments()
        {
            var context = GetInMemoryDbContext();
            var repo = new CommentRepository(context);

            var user = new User { UserID = 1, UserName = "test" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var workout = new Workout { WorkoutID = 1, Type = "Strength" };
            var comment1 = new Comment
            {
                CommentID = 1,
                UserID = user.UserID,
                WorkoutID = workout.WorkoutID,
                Text = "Hard workout!"
            };
            var comment2 = new Comment
            {
                CommentID = 2,
                UserID = user.UserID,
                WorkoutID = workout.WorkoutID,
                Text = "Felt strong!"
            };
            context.Workout.Add(workout);
            context.Comment.AddRange(comment1, comment2);
            await context.SaveChangesAsync();

            var result = await repo.GetAllCommentsByWorkoutAsync(workout.WorkoutID);

            Assert.NotNull(result);
            Assert.Equal(2, result!.Count);
            Assert.Contains(result, c => c.Text == "Hard workout!");
            Assert.Contains(result, c => c.Text == "Felt strong!");
        }

        [Fact]
        public async Task UpdateCommentAsync_ShouldUpdateComment()
        {
            var context = GetInMemoryDbContext();
            var repo = new CommentRepository(context);

            var user = new User { UserID = 1, UserName = "test" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var comment = new Comment
            {
                CommentID = 1,
                UserID = user.UserID,
                Text = "Great run!"
            };
            context.Comment.Add(comment);
            await context.SaveChangesAsync();

            comment.Text = "Amazing run!";
            var updatedComment = await repo.UpdateCommentAsync(comment);

            Assert.Equal("Amazing run!", updatedComment.Text);
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldRemoveComment()
        {
            var context = GetInMemoryDbContext();
            var repo = new CommentRepository(context);

            var user = new User { UserID = 1, UserName = "test" };
            context.User.Add(user);
            await context.SaveChangesAsync();

            var comment = new Comment
            {
                CommentID = 1,
                UserID = user.UserID,
                Text = "Great run!"
            };
            context.Comment.Add(comment);
            await context.SaveChangesAsync();

            var result = await repo.DeleteCommentAsync(1);

            Assert.True(result);
            Assert.Empty(context.Comment);
        }

        [Fact]
        public async Task DeleteCommentAsync_ShouldReturnFalse_IfCommentNotFound()
        {
            var context = GetInMemoryDbContext();
            var repo = new CommentRepository(context);

            var result = await repo.DeleteCommentAsync(999);

            Assert.False(result);
        }
    }
}
