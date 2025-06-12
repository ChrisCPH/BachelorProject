using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using RunningPlanner.Data;
using RunningPlanner.Models;
using System.Security.Claims;

public class CommentPermissionTests
{
    private static RunningPlannerDbContext GetDbContextWithData(string permission)
    {
        var db = DbContextFactory.CreateInMemoryContext();

        var run = new Run { RunID = 1, TrainingPlanID = 88 };
        var comment = new Comment { CommentID = 10, RunID = run.RunID };

        db.Run.Add(run);
        db.Comment.Add(comment);
        if (!string.IsNullOrEmpty(permission))
        {
            db.UserTrainingPlan.Add(new UserTrainingPlan
            {
                UserID = 123,
                TrainingPlanID = 88,
                Permission = permission
            });
        }
        db.SaveChanges();
        return db;
    }

    private static ActionExecutingContext GetContext(
        RunningPlannerDbContext dbContext,
        int userId,
        Dictionary<string, object> actionArguments)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim("UserID", userId.ToString())]));

        var httpContext = new DefaultHttpContext
        {
            User = user,
            RequestServices = new ServiceProviderMock(dbContext)
        };

        return new ActionExecutingContext(
            new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new ControllerActionDescriptor()),
            new List<IFilterMetadata>(),
            actionArguments!,
            controller: null!);
    }

    [Fact]
    public async Task ReturnsUnauthorized_If_UserIdMissing()
    {
        var dbContext = DbContextFactory.CreateInMemoryContext();
        var filter = new CommentPermissionAuthorize("editor");

        var context = new ActionExecutingContext(
            new ActionContext(new DefaultHttpContext(), new Microsoft.AspNetCore.Routing.RouteData(), new ControllerActionDescriptor()),
            new List<IFilterMetadata>(),
            new Dictionary<string, object>()!,
            controller: null!);

        await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null!));

        Assert.IsType<UnauthorizedResult>(context.Result);
    }

    [Fact]
    public async Task ReturnsNotFound_If_CommentNotFound()
    {
        var dbContext = DbContextFactory.CreateInMemoryContext();
        var filter = new CommentPermissionAuthorize("editor");

        var context = GetContext(dbContext, userId: 10, new Dictionary<string, object> { { "id", 999 } });

        await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null!));

        Assert.IsType<NotFoundObjectResult>(context.Result);
    }

    [Fact]
    public async Task ReturnsNotFound_If_CommentHasNoRun()
    {
        var dbContext = DbContextFactory.CreateInMemoryContext();
        dbContext.Comment.Add(new Comment { CommentID = 10 });
        dbContext.SaveChanges();

        var filter = new CommentPermissionAuthorize("editor");
        var context = GetContext(dbContext, userId: 1, new Dictionary<string, object> { { "id", 10 } });

        await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null!));

        Assert.IsType<BadRequestObjectResult>(context.Result);
    }

    [Fact]
    public async Task ReturnsForbid_WhenUserHasNoTrainingPlanEntry()
    {
        var db = GetDbContextWithData("owner");
        db.UserTrainingPlan.RemoveRange(db.UserTrainingPlan);
        db.SaveChanges();

        var context = GetContext(db, 123, new() { { "id", 10 } });

        var filter = new CommentPermissionAuthorize("editor");
        await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null!));

        Assert.IsType<ForbidResult>(context.Result);
    }

    [Fact]
    public async Task ReturnsUnauthorized_WhenUserHasWrongPermission()
    {
        var db = GetDbContextWithData("viewer");
        var context = GetContext(db, 123, new() { { "id", 10 } });

        var filter = new CommentPermissionAuthorize("owner");
        await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null!));

        var result = Assert.IsType<UnauthorizedObjectResult>(context.Result);
        var message = result.Value!.ToString()!;
        Assert.Contains("required permissions", message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("owner", message);
    }

    [Theory]
    [InlineData("owner")]
    [InlineData("editor")]
    [InlineData("commenter")]
    public async Task AllowsExecution_WhenPermissionIsValid(string validPermission)
    {
        var dbContext = GetDbContextWithData(validPermission);

        var filter = new CommentPermissionAuthorize("owner", "editor", "commenter");
        var context = GetContext(dbContext, userId: 123, new Dictionary<string, object> { { "id", 10 } });

        bool nextCalled = false;
        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        });

        Assert.True(nextCalled);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task ResolvesCommentId_FromCommentObjectDirectly()
    {
        var db = GetDbContextWithData("owner");
        var comment = db.Comment.First();

        var context = GetContext(db, 123, new() { { "model", comment } });

        var filter = new CommentPermissionAuthorize("owner");

        bool nextCalled = false;
        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult<ActionExecutedContext>(null!);
        });

        Assert.True(nextCalled);
        Assert.Null(context.Result);
    }
}