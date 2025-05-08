using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using RunningPlanner.Data;

public class TrainingPlanPermissionAuthorize : AuthorizeAttribute, IAsyncActionFilter
{
    private readonly string[] _allowedPermissions;

    public TrainingPlanPermissionAuthorize(params string[] allowedPermissions)
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

        int? trainingPlanId = null;

        // Try to get from route (e.g. /trainingplan/{id})
        if (context.ActionArguments.TryGetValue("id", out var routeIdObj) && routeIdObj is int routeId)
        {
            trainingPlanId = routeId;
        }
        else
        {
            // Otherwise, try to get TrainingPlanID from a model in the request body
            foreach (var arg in context.ActionArguments.Values)
            {
                if (arg == null) continue;

                var prop = arg.GetType().GetProperty("TrainingPlanID");
                if (prop != null && prop.GetValue(arg) is int bodyId)
                {
                    trainingPlanId = bodyId;
                    break;
                }
            }
        }

        if (trainingPlanId == null)
        {
            context.Result = new BadRequestObjectResult("Training plan ID is missing.");
            return;
        }

        var dbContext = context.HttpContext.RequestServices.GetService<RunningPlannerDbContext>();
        if (dbContext == null)
        {
            context.Result = new StatusCodeResult(500);
            return;
        }

        var userPlan = await dbContext.UserTrainingPlan
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.UserID == userId && up.TrainingPlanID == trainingPlanId.Value);

        if (userPlan == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        if (!_allowedPermissions.Contains(userPlan.Permission, StringComparer.OrdinalIgnoreCase))
        {
            context.Result = new UnauthorizedObjectResult(new { message = $"You do not have the required permissions. Required: {string.Join(", ", _allowedPermissions)}" });
            return;
        }

        await next();
    }
}
