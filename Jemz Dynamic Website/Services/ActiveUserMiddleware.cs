using System.Security.Claims;
using JemzCoffee.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace JemzCoffee.Services;

public class ActiveUserMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> users)
    {
        var path = context.Request.Path;
        var isStaticAsset = path.StartsWithSegments("/css") || path.StartsWithSegments("/js") ||
                            path.StartsWithSegments("/lib") || path.StartsWithSegments("/favicon");
        if (!isStaticAsset && context.User.Identity?.IsAuthenticated == true)
        {
            var id = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = id is null ? null : await users.FindByIdAsync(id);
            if (user is null || !user.IsActive)
            {
                await context.SignOutAsync(IdentityConstants.ApplicationScheme);
                context.User = new ClaimsPrincipal(new ClaimsIdentity());
            }
        }

        await next(context);
    }
}
