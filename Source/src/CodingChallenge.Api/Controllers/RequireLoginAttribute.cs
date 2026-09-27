using CodingChallenge.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CodingChallenge.Api.Controllers;

/// <summary>Rejects requests that do not carry a valid access token in the <c>X-Auth-Token</c> header.</summary>
public class RequireLoginAttribute : ActionFilterAttribute
{
    public const string TokenHeader = "X-Auth-Token";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var authService = context.HttpContext.RequestServices.GetRequiredService<IAuthService>();
        var token = context.HttpContext.Request.Headers[TokenHeader].FirstOrDefault();
        var user = authService.ResolveUser(token);

        if (user is null)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Missing or invalid access token." });
            return;
        }

        context.HttpContext.Items["User"] = user;
        base.OnActionExecuting(context);
    }
}
