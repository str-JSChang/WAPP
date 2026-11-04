using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Muzik67.Helpers
{
    public static class SessionAuthorization
    {
        public static IActionResult? RequireMember(PageModel page)
        {
            if (page.HttpContext.Session.GetInt32("UserId") is null)
            {
                var returnUrl = $"{page.Request.Path}{page.Request.QueryString}";
                return page.RedirectToPage("/Account/Login", new { returnUrl });
            }

            if (!string.Equals(page.HttpContext.Session.GetString("Role"), "Member", StringComparison.Ordinal))
            {
                return page.RedirectToPage("/Account/AccessDenied");
            }

            return null;
        }

        public static IActionResult? RequireAdmin(PageModel page)
        {
            if (page.HttpContext.Session.GetInt32("UserId") is null)
            {
                var returnUrl = $"{page.Request.Path}{page.Request.QueryString}";
                return page.RedirectToPage("/Account/Login", new { returnUrl });
            }

            if (!string.Equals(page.HttpContext.Session.GetString("Role"), "Admin", StringComparison.Ordinal))
            {
                return page.RedirectToPage("/Account/AccessDenied");
            }

            return null;
        }
    }
}
