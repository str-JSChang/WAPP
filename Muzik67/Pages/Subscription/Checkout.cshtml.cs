using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;
using Muzik67.Models;
using Muzik67.Pages.Shared;

namespace Muzik67.Pages.Subscription
{
    public class CheckoutModel : SessionPageModel
    {
        private readonly MuzikDbContext _context;

        public CheckoutModel(MuzikDbContext context)
        {
            _context = context;
        }

        public SubscriptionPlan? Plan { get; private set; }
        protected override bool MemberOnly => true;

        public async Task<IActionResult> OnGetAsync(int planId)
        {
            Plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(plan => plan.PlanId == planId && plan.Status == "Active");
            if (Plan is null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostConfirmAsync(int planId)
        {
            // ALL OR NOTHING no one wants to be emotionally half subscribed
            await using var transaction = await _context.Database.BeginTransactionAsync();

            var plan = await _context.SubscriptionPlans
                .FirstOrDefaultAsync(item => item.PlanId == planId && item.Status == "Active");
            if (plan is null)
            {
                return NotFound();
            }

            var startDate = DateTime.UtcNow;
            if (plan.Duration is not ("Monthly" or "Yearly"))
            {
                return BadRequest("The plan has an unsupported billing period.");
            }

            var endDate = plan.Duration == "Monthly"
                ? startDate.AddMonths(1)
                : startDate.AddYears(1);
            var userId = CurrentUserId;

            var existingSubscriptions = await _context.UserSubscriptions
                .Where(subscription => subscription.UserId == userId && subscription.Status == "Active")
                .ToListAsync();
            if (existingSubscriptions.Any(subscription =>
                subscription.PlanId == plan.PlanId && subscription.EndDate > startDate))
            {
                TempData["StatusMessage"] = "You already have this active plan.";
                return RedirectToPage("/Subscription/Plans");
            }

            foreach (var existing in existingSubscriptions)
            {
                existing.Status = existing.EndDate <= startDate ? "Expired" : "Cancelled";
            }

            _context.UserSubscriptions.Add(new UserSubscription
            {
                UserId = userId,
                PlanId = plan.PlanId,
                StartDate = startDate,
                EndDate = endDate,
                Status = "Active"
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // DEMO MONEY ONLY please do not ask where the bank API is hiding
            TempData["StatusMessage"] = "Your subscription is active. This was a demo checkout; no payment was processed.";
            return RedirectToPage("/Subscription/Plans");
        }
    }
}
