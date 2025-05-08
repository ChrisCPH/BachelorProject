using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using RunningPlanner.Data;

public class FeedbackPermissionAuthorize : AuthorizeAttribute, IAsyncActionFilter
{
    private readonly string[] _allowedPermissions;

    public FeedbackPermissionAuthorize(params string[] allowedPermissions)
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

        int? feedbackId = null;

        if (context.ActionArguments.TryGetValue("id", out var routeIdObj) && routeIdObj is int routeId)
        {
            feedbackId = routeId;
        }
        else
        {
            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg == null) continue;

                var prop = arg.GetType().GetProperty("FeedbackID");
                if (prop != null && prop.GetValue(arg) is int bodyId)
                {
                    feedbackId = bodyId;
                    break;
                }
            }
        }

        if (feedbackId == null)
        {
            context.Result = new BadRequestObjectResult("Feedback ID is missing.");
            return;
        }


        var dbContext = context.HttpContext.RequestServices.GetService<RunningPlannerDbContext>();
        if (dbContext == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        var feedback = await dbContext.Feedback.FirstOrDefaultAsync(r => r.FeedbackID == feedbackId);
        if (feedback == null)
        {
            context.Result = new NotFoundObjectResult("Feedback not found.");
            return;
        }

        // Detach it to avoid tracking conflict
        dbContext.Entry(feedback).State = EntityState.Detached;

        var run = await dbContext.Run
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.RunID == feedback.RunID);

        if (run == null)
        {
            context.Result = new NotFoundObjectResult("Run not found.");
            return;
        }

        var userPermission = await dbContext.UserTrainingPlan
            .FirstOrDefaultAsync(utp => utp.UserID == userId && utp.TrainingPlanID == run.TrainingPlanID);

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
