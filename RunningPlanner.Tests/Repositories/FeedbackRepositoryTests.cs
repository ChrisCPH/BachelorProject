using RunningPlanner.Models;
using RunningPlanner.Repositories;
using Microsoft.EntityFrameworkCore;

namespace RunningPlanner.Tests
{
    public class FeedbackRepositoryTests : TestBase
    {
        [Fact]
        public async Task AddFeedbackAsync_ShouldAddFeedback()
        {
            var context = GetInMemoryDbContext();
            var repo = new FeedbackRepository(context);

            var run = new Run { RunID = 1, Distance = 10 };
            context.Run.Add(run);
            await context.SaveChangesAsync();

            var feedback = new Feedback
            {
                FeedbackID = 1,
                RunID = run.RunID,
                EffortRating = 4,
                FeelRating = 8,
                Comment = "Great run!"
            };

            var result = await repo.AddFeedbackAsync(feedback);

            Assert.NotNull(result);
            Assert.Equal(1, context.Feedback.Count());
            Assert.Equal("Great run!", result.Comment);
        }

        [Fact]
        public async Task GetFeedbackByIdAsync_ShouldReturnFeedback()
        {
            var context = GetInMemoryDbContext();
            var repo = new FeedbackRepository(context);
            
            var run = new Run { Distance = 10 };
            context.Run.Add(run);
            await context.SaveChangesAsync();

            var feedback = new Feedback
            {
                RunID = run.RunID,
                EffortRating = 4,
                FeelRating = 8,
                Comment = "Great run!"
            };
            context.Feedback.Add(feedback);
            await context.SaveChangesAsync();

            var result = await repo.GetFeedbackByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal("Great run!", result!.Comment);
        }

        [Fact]
        public async Task GetFeedbackByRunAsync_ShouldReturnFeedback()
        {
            var context = GetInMemoryDbContext();
            var repo = new FeedbackRepository(context);

            var run = new Run { Distance = 10 };
            context.Run.Add(run);
            await context.SaveChangesAsync();

            var feedback = new Feedback
            {
                RunID = run.RunID,
                EffortRating = 4,
                FeelRating = 8,
                Comment = "Great run!"
            };
            context.Feedback.Add(feedback);
            await context.SaveChangesAsync();


            var result = await repo.GetFeedbackByRunAsync(run.RunID);

            Assert.NotNull(result);
            Assert.Equal("Great run!", result!.Comment);
        }

        [Fact]
        public async Task UpdateFeedbackAsync_ShouldUpdateFeedback()
        {
            var context = GetInMemoryDbContext();
            var repo = new FeedbackRepository(context);

            var run = new Run { Distance = 10 };
            context.Run.Add(run);
            await context.SaveChangesAsync();

            var feedback = new Feedback
            {
                RunID = run.RunID,
                EffortRating = 4,
                FeelRating = 8,
                Comment = "Great run!"
            };
            context.Feedback.Add(feedback);
            await context.SaveChangesAsync();

            feedback.Comment = "Updated feedback!";
            var updatedFeedback = await repo.UpdateFeedbackAsync(feedback);

            Assert.Equal("Updated feedback!", updatedFeedback.Comment);
        }

        [Fact]
        public async Task DeleteFeedbackAsync_ShouldRemoveFeedback()
        {
            var context = GetInMemoryDbContext();
            var repo = new FeedbackRepository(context);

            var run = new Run { Distance = 10 };
            context.Run.Add(run);
            await context.SaveChangesAsync();

            var feedback = new Feedback
            {
                RunID = run.RunID,
                EffortRating = 4,
                FeelRating = 8,
                Comment = "Great run!"
            };
            context.Feedback.Add(feedback);
            await context.SaveChangesAsync();

            var result = await repo.DeleteFeedbackAsync(1);

            Assert.True(result);
            Assert.Empty(context.Feedback);
        }

        [Fact]
        public async Task DeleteFeedbackAsync_ShouldReturnFalse_IfFeedbackNotFound()
        {
            var context = GetInMemoryDbContext();
            var repo = new FeedbackRepository(context);

            var result = await repo.DeleteFeedbackAsync(999);

            Assert.False(result);
        }
    }
}
