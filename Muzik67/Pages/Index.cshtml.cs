using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Muzik67.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId is null or <= 0)
        {
            return Page();
        }

        var role = HttpContext.Session.GetString("Role");
        if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToPage("/Admin/Dashboard");
        }

        if (string.Equals(role, "Member", StringComparison.OrdinalIgnoreCase))
        {
            return RedirectToPage("/Student/StudentDashboard");
        }

        _logger.LogWarning("Clearing session for user ID {UserId} because role '{Role}' is not recognised.", userId, role);
        HttpContext.Session.Clear();
        return Page();
    }
}
