using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;
using Muzik67.Models;

namespace Muzik67.Pages.Student
{
    public class SubscriptionModel : PageModel
    {
        private readonly MuzikDbContext _context;

        public SubscriptionModel(MuzikDbContext context)
        {
            _context = context;
        }

        public UserSubscription? CurrentSubscription { get; private set; }
        public IList<UserSubscription> History { get; private set; } = new List<UserSubscription>();

        public async Task<IActionResult> OnGetAsync()
        {
            var access = SessionAuthorization.RequireMember(this);
            if (access is not null)
            {
                return access;
            }

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;
            CurrentSubscription = await SubscriptionAccess.GetActiveAsync(_context, userId);
            History = await _context.UserSubscriptions
                .Where(subscription => subscription.UserId == userId)
                .Include(subscription => subscription.Plan)
                .OrderByDescending(subscription => subscription.StartDate)
                .Take(10)
                .ToListAsync();

            return Page();
        }
    }
}
