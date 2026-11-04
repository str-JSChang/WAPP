using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;

namespace Muzik67.Pages.Shared;

public abstract class SessionPageModel : PageModel
{
    protected int CurrentUserId { get; private set; }
    protected virtual bool AdminOnly => false;
    protected virtual bool MemberOnly => false;

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

        var db = HttpContext.RequestServices.GetRequiredService<MuzikDbContext>();
        var user = await db.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.UserId == userId.Value);

        if (user is null || !string.Equals(user.AccountStatus, "Active", StringComparison.OrdinalIgnoreCase))
        {
            HttpContext.Session.Clear();
            context.Result = new RedirectToPageResult(
                "/Account/Login",
                new { returnUrl = $"{Request.Path}{Request.QueryString}", accountDisabled = user is not null });
            return;
        }

        var requiredRole = AdminOnly ? "Admin" : MemberOnly ? "Member" : null;
        if (requiredRole is not null &&
            !string.Equals(user.Role, requiredRole, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new RedirectToPageResult("/Account/AccessDenied");
            return;
        }

        HttpContext.Session.SetString("Username", user.Username);
        HttpContext.Session.SetString("Role", user.Role);
        CurrentUserId = userId.Value;
        await next();
    }
}
