using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using RunningPlanner.Data;

public class ExercisePermissionAuthorize : AuthorizeAttribute, IAsyncActionFilter
{
    private readonly string[] _allowedPermissions;

    public ExercisePermissionAuthorize(params string[] allowedPermissions)
    {
        _allowedPermissions = allowedPermissions;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userIdClaim = context.HttpContext.User.FindFirst("UserID");
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        int? exerciseId = null;

        if (context.ActionArguments.TryGetValue("id", out var routeIdObj) && routeIdObj is int routeId)
        {
            exerciseId = routeId;
        }
        else
        {
            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg == null) continue;

                var prop = arg.GetType().GetProperty("ExerciseID");
                if (prop != null && prop.GetValue(arg) is int bodyId)
                {
                    exerciseId = bodyId;
                    break;
                }
            }
        }

        var dbContext = context.HttpContext.RequestServices.GetService<RunningPlannerDbContext>();
        if (dbContext == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        var exercise = await dbContext.Exercise.FirstOrDefaultAsync(r => r.ExerciseID == exerciseId);
        if (exercise == null)
        {
            context.Result = new NotFoundObjectResult("Exercise not found.");
            return;
        }

        // Detach it to avoid tracking conflict
        dbContext.Entry(exercise).State = EntityState.Detached;

        var workout = await dbContext.Workout
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.WorkoutID == exercise.WorkoutID);

        if (workout == null)
        {
            context.Result = new NotFoundObjectResult("Workout not found.");
            return;
        }

        var userPermission = await dbContext.UserTrainingPlan
            .FirstOrDefaultAsync(utp => utp.UserID == userId && utp.TrainingPlanID == workout.TrainingPlanID);

        if (userPermission == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        if (!_allowedPermissions.Contains(userPermission.Permission, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new UnauthorizedObjectResult(new { message = $"You do not have the required permissions. Required: {string.Join(", ", _allowedPermissions)}" });
            return;
        }

        await next();
    }
}
