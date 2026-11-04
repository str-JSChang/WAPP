using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;
using Muzik67.Models;
using Muzik67.Pages.Shared;

namespace Muzik67.Pages.Student
{
    public class SubscriptionModel : SessionPageModel
    {
        private readonly MuzikDbContext _context;

        public SubscriptionModel(MuzikDbContext context)
        {
            _context = context;
        }

        public UserSubscription? CurrentSubscription { get; private set; }
        protected override bool MemberOnly => true;
        public IList<UserSubscription> History { get; private set; } = new List<UserSubscription>();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = CurrentUserId;
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
