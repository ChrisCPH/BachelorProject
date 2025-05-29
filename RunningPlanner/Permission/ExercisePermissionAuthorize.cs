using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using RunningPlanner.Data;
using RunningPlanner.Models;

public class ExercisePermissionAuthorize : AuthorizeAttribute, IAsyncActionFilter
{
    private readonly string[] _allowedPermissions;

    public ExercisePermissionAuthorize(params string[] allowedPermissions)
    {
        _allowedPermissions = allowedPermissions;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userId = GetUserIdFromContext(context);
        if (userId == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var dbContext = context.HttpContext.RequestServices.GetService<RunningPlannerDbContext>();
        if (dbContext == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        var exerciseId = ResolveExerciseId(context);
        if (exerciseId == null)
        {
            context.Result = new BadRequestObjectResult("Missing or invalid Exercise ID.");
            return;
        }

        var exercise = await GetExerciseAsync(dbContext, exerciseId.Value);
        if (exercise == null)
        {
            context.Result = new NotFoundObjectResult("Exercise not found.");
            return;
        }

        var workout = await dbContext.Workout
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.WorkoutID == exercise.WorkoutID);

        if (workout == null)
        {
            context.Result = new NotFoundObjectResult("Workout not found.");
            return;
        }

        var (isUserFound, hasCorrectPermission) = await HasPermissionAsync(userId.Value, workout.TrainingPlanID, dbContext);

        if (!isUserFound)
        {
            context.Result = new ForbidResult();
            return;
        }

        if (!hasCorrectPermission)
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = $"You do not have the required permissions. Required: {string.Join(", ", _allowedPermissions)}"
            });
            return;
        }

        await next();
    }

    private int? GetUserIdFromContext(ActionExecutingContext context)
    {
        var userIdClaim = context.HttpContext.User.FindFirst("UserID");
        return userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId)
            ? userId
            : null;
    }

    private int? ResolveExerciseId(ActionExecutingContext context)
    {
        if (context.ActionArguments.TryGetValue("id", out var routeIdObj) && routeIdObj is int routeId)
        {
            return routeId;
        }

        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg == null) continue;

            var prop = arg.GetType().GetProperty("ExerciseID");
            if (prop != null && prop.GetValue(arg) is int bodyId)
            {
                return bodyId;
            }
        }

        return null;
    }

    private async Task<Exercise?> GetExerciseAsync(RunningPlannerDbContext dbContext, int exerciseId)
    {
        var exercise = await dbContext.Exercise.FirstOrDefaultAsync(e => e.ExerciseID == exerciseId);
        if (exercise != null)
        {
            dbContext.Entry(exercise).State = EntityState.Detached;
        }
        return exercise;
    }

    private async Task<(bool isUserFound, bool hasCorrectPermission)> HasPermissionAsync(int userId, int trainingPlanId, RunningPlannerDbContext dbContext)
    {
        var userPermission = await dbContext.UserTrainingPlan
            .AsNoTracking()
            .FirstOrDefaultAsync(utp => utp.UserID == userId && utp.TrainingPlanID == trainingPlanId);

        if (userPermission == null)
        {
            return (false, false);
        }

        var hasCorrectPermission = _allowedPermissions.Contains(userPermission.Permission, StringComparer.OrdinalIgnoreCase);
        return (true, hasCorrectPermission);
    }
}
