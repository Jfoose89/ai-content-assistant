using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ServiceA.ContentApi.Filters;

/// <summary>
/// Automatically validates ModelState before each action executes.
/// Returns 400 Bad Request with validation details if ModelState is invalid,
/// eliminating repetitive ModelState checks in controllers.
/// </summary>
public class ValidationFilter : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }
}