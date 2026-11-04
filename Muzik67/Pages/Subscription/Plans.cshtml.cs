using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;
using Muzik67.Models;
using Muzik67.Pages.Shared;

namespace Muzik67.Pages.Subscription
{
    public class PlansModel : SessionPageModel
    {
        private readonly MuzikDbContext _context;

        public PlansModel(MuzikDbContext context)
        {
            _context = context;
        }

        public IList<SubscriptionPlan> Plans { get; private set; } = new List<SubscriptionPlan>();
        protected override bool MemberOnly => true;
        public UserSubscription? CurrentSubscription { get; private set; }

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = CurrentUserId;
            var plans = await _context.SubscriptionPlans
                .Where(plan => plan.Status == "Active")
                .ToListAsync();
            Plans = plans
                .OrderBy(plan => plan.Price)
                .ThenBy(plan => plan.PlanName)
                .ToList();
            CurrentSubscription = await SubscriptionAccess.GetActiveAsync(_context, userId);

            return Page();
        }
    }
}
