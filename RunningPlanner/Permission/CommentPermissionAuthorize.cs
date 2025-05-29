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

        var comment = await ResolveCommentAsync(context, dbContext);
        if (comment == null)
            return;

        var trainingPlanId = await GetTrainingPlanIdAsync(comment, dbContext);
        if (trainingPlanId == null)
        {
            context.Result = new BadRequestObjectResult("Comment must be linked to a Run or Workout.");
            return;
        }

        var hasPermission = await HasPermissionAsync(userId.Value, trainingPlanId.Value, dbContext);
        if (!hasPermission.isUserFound)
        {
            context.Result = new ForbidResult();
            return;
        }

        if (!hasPermission.hasCorrectPermission)
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

    private async Task<Comment?> ResolveCommentAsync(ActionExecutingContext context, RunningPlannerDbContext dbContext)
    {
        if (context.ActionArguments.Values.FirstOrDefault(v => v is Comment) is Comment commentFromBody)
        {
            return commentFromBody;
        }

        if (!context.ActionArguments.TryGetValue("id", out var idObj) || idObj is not int commentId)
        {
            context.Result = new BadRequestObjectResult("Missing or invalid Comment ID.");
            return null;
        }

        var comment = await dbContext.Comment.AsNoTracking().FirstOrDefaultAsync(c => c.CommentID == commentId);
        if (comment == null)
        {
            context.Result = new NotFoundObjectResult("Comment not found.");
            return null;
        }

        return comment;
    }

    private async Task<int?> GetTrainingPlanIdAsync(Comment comment, RunningPlannerDbContext dbContext)
    {
        if (comment.WorkoutID.HasValue)
        {
            var workout = await dbContext.Workout
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.WorkoutID == comment.WorkoutID.Value);

            return workout?.TrainingPlanID;
        }

        if (comment.RunID.HasValue)
        {
            var run = await dbContext.Run
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.RunID == comment.RunID.Value);

            return run?.TrainingPlanID;
        }

        return null;
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
