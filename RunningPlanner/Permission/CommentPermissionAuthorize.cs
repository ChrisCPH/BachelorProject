using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using RunningPlanner.Data;
using RunningPlanner.Models;

public class CommentPermissionAuthorize : AuthorizeAttribute, IAsyncActionFilter
{
    private readonly string[] _allowedPermissions;

    public CommentPermissionAuthorize(params string[] allowedPermissions)
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

        var dbContext = context.HttpContext.RequestServices.GetService<RunningPlannerDbContext>();
        if (dbContext == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        Comment? comment = null;
        int? trainingPlanId = null;

        comment = context.ActionArguments.Values
            .FirstOrDefault(v => v is Comment) as Comment;

        if (comment != null)
        {
            if (comment.WorkoutID.HasValue)
            {
                var workout = await dbContext.Workout
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.WorkoutID == comment.WorkoutID.Value);

                if (workout == null)
                {
                    context.Result = new NotFoundObjectResult("Workout not found.");
                    return;
                }

                trainingPlanId = workout.TrainingPlanID;
            }
            else if (comment.RunID.HasValue)
            {
                var run = await dbContext.Run
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.RunID == comment.RunID.Value);

                if (run == null)
                {
                    context.Result = new NotFoundObjectResult("Run not found.");
                    return;
                }

                trainingPlanId = run.TrainingPlanID;
            }
            else
            {
                context.Result = new BadRequestObjectResult("Comment must be linked to a Run or Workout.");
                return;
            }
        }
        else
        {
            if (!context.ActionArguments.TryGetValue("id", out var idObj) || idObj is not int commentId)
            {
                context.Result = new BadRequestObjectResult("Missing or invalid Comment ID.");
                return;
            }

            comment = await dbContext.Comment.AsNoTracking()
                .FirstOrDefaultAsync(c => c.CommentID == commentId);

            if (comment == null)
            {
                context.Result = new NotFoundObjectResult("Comment not found.");
                return;
            }

            if (comment.WorkoutID.HasValue)
            {
                var workout = await dbContext.Workout
                    .AsNoTracking()
                    .FirstOrDefaultAsync(w => w.WorkoutID == comment.WorkoutID.Value);

                if (workout == null)
                {
                    context.Result = new NotFoundObjectResult("Workout not found.");
                    return;
                }

                trainingPlanId = workout.TrainingPlanID;
            }
            else if (comment.RunID.HasValue)
            {
                var run = await dbContext.Run
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.RunID == comment.RunID.Value);

                if (run == null)
                {
                    context.Result = new NotFoundObjectResult("Run not found.");
                    return;
                }

                trainingPlanId = run.TrainingPlanID;
            }
            else
            {
                context.Result = new BadRequestObjectResult("Comment is not linked to a Run or Workout.");
                return;
            }
        }

        var userPermission = await dbContext.UserTrainingPlan
            .FirstOrDefaultAsync(utp => utp.UserID == userId && utp.TrainingPlanID == trainingPlanId);

        if (userPermission == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        if (!_allowedPermissions.Contains(userPermission.Permission, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = $"You do not have the required permissions. Required: {string.Join(", ", _allowedPermissions)}"
            });
            return;
        }

        await next();
    }
}
