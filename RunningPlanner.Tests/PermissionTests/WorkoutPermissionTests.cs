using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RunningPlanner.Data;
using RunningPlanner.Models;
using System.Security.Claims;

public class WorkoutPermissionAuthorizeTests
{
    private RunningPlannerDbContext GetDbContextWithData(string permission, int workoutId = 99, int trainingPlanId = 77)
    {
        var options = new DbContextOptionsBuilder<RunningPlannerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new RunningPlannerDbContext(options);

        var workout = new Workout
        {
            WorkoutID = workoutId,
            TrainingPlanID = trainingPlanId
        };
        context.Workout.Add(workout);

        var userTrainingPlan = new UserTrainingPlan
        {
            UserID = 123,
            TrainingPlanID = trainingPlanId,
            Permission = permission
        };
        context.UserTrainingPlan.Add(userTrainingPlan);

        context.SaveChanges();
        return context;
    }

    private ActionExecutingContext GetContext(
        RunningPlannerDbContext dbContext,
        int? userId,
        Dictionary<string, object> actionArguments)
    {
        var claims = userId.HasValue
            ? new List<Claim> { new Claim("UserID", userId.Value.ToString()) }
            : new List<Claim>();

        var identity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext
        {
            User = user,
            RequestServices = new ServiceCollection()
                .AddSingleton(dbContext)
                .BuildServiceProvider()
        };

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
            ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
        };

        return new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            actionArguments!,
            controller: null!
        );
    }

    [Fact]
    public async Task ReturnsUnauthorized_WhenUserMissing()
    {
        var db = GetDbContextWithData("owner");
        var context = GetContext(db, null, new() { { "id", 99 } });

        var filter = new WorkoutPermissionAuthorize("owner");
        await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null!));

        Assert.IsType<UnauthorizedResult>(context.Result);
    }

    [Fact]
    public async Task ReturnsBadRequest_WhenWorkoutIdMissing()
    {
        var db = GetDbContextWithData("owner");
        var context = GetContext(db, 123, new());

        var filter = new WorkoutPermissionAuthorize("owner");
        await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null!));

        var result = Assert.IsType<BadRequestObjectResult>(context.Result);
        Assert.Equal("Workout ID is missing.", result.Value);
    }

    [Fact]
    public async Task ReturnsNotFound_WhenWorkoutDoesNotExist()
    {
        var db = GetDbContextWithData("owner");
        var context = GetContext(db, 123, new() { { "id", 999 } });

        var filter = new WorkoutPermissionAuthorize("owner");
        await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null!));

        var result = Assert.IsType<NotFoundObjectResult>(context.Result);
        Assert.Equal("Workout not found.", result.Value);
    }

    [Fact]
    public async Task ReturnsForbid_WhenUserHasNoTrainingPlanEntry()
    {
        var db = GetDbContextWithData("owner");
        db.UserTrainingPlan.RemoveRange(db.UserTrainingPlan);
        db.SaveChanges();

        var context = GetContext(db, 123, new() { { "id", 99 } });

        var filter = new WorkoutPermissionAuthorize("owner");
        await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null!));

        Assert.IsType<ForbidResult>(context.Result);
    }

    [Fact]
    public async Task ReturnsUnauthorized_WhenUserHasWrongPermission()
    {
        var db = GetDbContextWithData("viewer");
        var context = GetContext(db, 123, new() { { "id", 99 } });

        var filter = new WorkoutPermissionAuthorize("owner");
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
        var db = GetDbContextWithData(validPermission);
        var context = GetContext(db, 123, new() { { "id", 99 } });

        var filter = new WorkoutPermissionAuthorize("owner", "editor", "commenter");

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
    public async Task ResolvesWorkoutId_FromActionArgumentObjectProperty()
    {
        var db = GetDbContextWithData("owner");

        var workoutInput = new { WorkoutID = 99 };
        var context = GetContext(db, 123, new() { { "model", workoutInput } });

        var filter = new WorkoutPermissionAuthorize("owner");

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
