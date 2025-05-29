using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using RunningPlanner.Data;
using RunningPlanner.Models;

public class FeedbackPermissionAuthorize : AuthorizeAttribute, IAsyncActionFilter
{
    private readonly string[] _allowedPermissions;

    public FeedbackPermissionAuthorize(params string[] allowedPermissions)
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

        var feedbackId = ResolveFeedbackId(context);
        if (feedbackId == null)
        {
            context.Result = new BadRequestObjectResult("Feedback ID is missing.");
            return;
        }

        var feedback = await GetFeedbackAsync(dbContext, feedbackId.Value);
        if (feedback == null)
        {
            context.Result = new NotFoundObjectResult("Feedback not found.");
            return;
        }

        var run = await dbContext.Run
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RunID == feedback.RunID);

        if (run == null)
        {
            context.Result = new NotFoundObjectResult("Run not found.");
            return;
        }

        var (isUserFound, hasCorrectPermission) = await HasPermissionAsync(userId.Value, run.TrainingPlanID, dbContext);

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

    private int? ResolveFeedbackId(ActionExecutingContext context)
    {
        if (context.ActionArguments.TryGetValue("id", out var idObj) && idObj is int id)
        {
            return id;
        }

        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg == null) continue;

            var prop = arg.GetType().GetProperty("FeedbackID");
            if (prop != null && prop.GetValue(arg) is int bodyId)
            {
                return bodyId;
            }
        }

        return null;
    }

    private async Task<Feedback?> GetFeedbackAsync(RunningPlannerDbContext dbContext, int feedbackId)
    {
        var feedback = await dbContext.Feedback.FirstOrDefaultAsync(f => f.FeedbackID == feedbackId);
        if (feedback != null)
        {
            dbContext.Entry(feedback).State = EntityState.Detached;
        }
        return feedback;
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
