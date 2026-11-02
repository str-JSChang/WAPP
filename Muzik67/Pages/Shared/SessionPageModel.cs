using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Muzik67.Pages.Shared;

public abstract class SessionPageModel : PageModel
{
    protected int CurrentUserId { get; private set; }
    protected virtual bool AdminOnly => false;

    public override async Task OnPageHandlerExecutionAsync(
        PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null or <= 0)
        {
            var returnUrl = $"{Request.Path}{Request.QueryString}";
            context.Result = new RedirectToPageResult(
                "/Account/Login",
                new { returnUrl });
            return;
        }

        if (AdminOnly &&
            !string.Equals(
                HttpContext.Session.GetString("Role"),
                "Admin",
                StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new RedirectToPageResult("/Account/AccessDenied");
            return;
        }

        CurrentUserId = userId.Value;
        await next();
    }
}
