using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RunningPlanner.Data;
using RunningPlanner.Models;
using System.Security.Claims;

public class ExercisePermissionTests
{
    private RunningPlannerDbContext GetDbContextWithData(string permission)
    {
        var options = new DbContextOptionsBuilder<RunningPlannerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new RunningPlannerDbContext(options);

        var workout = new Workout { WorkoutID = 1, TrainingPlanID = 99 };
        var exercise = new Exercise { ExerciseID = 10, WorkoutID = workout.WorkoutID };
        var userTrainingPlan = new UserTrainingPlan
        {
            UserID = 123,
            TrainingPlanID = 99,
            Permission = permission
        };

        context.Workout.Add(workout);
        context.Exercise.Add(exercise);
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
        var db = GetDbContextWithData("editor");
        var context = GetContext(db, null, new() { { "id", 10 } });

        var filter = new ExercisePermissionAuthorize("editor");
        await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null!));

        Assert.IsType<UnauthorizedResult>(context.Result);
    }

    [Fact]
    public async Task ReturnsNotFound_WhenExerciseNotFound()
    {
        var db = GetDbContextWithData("editor");
        var context = GetContext(db, 123, new() { { "id", 999 } });

        var filter = new ExercisePermissionAuthorize("editor");
        await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null!));

        Assert.IsType<NotFoundObjectResult>(context.Result);
    }

    [Fact]
    public async Task ReturnsUnauthorized_WhenUserHasWrongPermission()
    {
        var db = GetDbContextWithData("viewer");
        var context = GetContext(db, 123, new() { { "id", 10 } });

        var filter = new ExercisePermissionAuthorize("editor");
        await filter.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null!));

        var result = Assert.IsType<UnauthorizedObjectResult>(context.Result);
        var message = result.Value!.ToString()!;
        Assert.Contains("required permissions", message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("editor", message);
    }

    [Theory]
    [InlineData("owner")]
    [InlineData("editor")]
    [InlineData("commenter")]
    public async Task AllowsExecution_WhenPermissionIsValid(string validPermission)
    {
        var db = GetDbContextWithData(validPermission);
        var context = GetContext(db, 123, new() { { "id", 10 } });

        var filter = new ExercisePermissionAuthorize("owner", "editor", "commenter");

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
    public async Task ResolvesExerciseId_FromActionArgumentObjectProperty()
    {
        var db = GetDbContextWithData("owner");

        var exerciseInput = new { ExerciseID = 10 };
        var context = GetContext(db, 123, new() { { "model", exerciseInput } });

        var filter = new ExercisePermissionAuthorize("owner");

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
