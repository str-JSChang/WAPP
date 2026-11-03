using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;
using Muzik67.Models;

namespace Muzik67.Pages.Admin
{
    public class SubscriptionManagementModel : PageModel
    {
        private readonly MuzikDbContext _context;

        public SubscriptionManagementModel(MuzikDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? PlanId { get; set; }

        public IList<SubscriptionPlan> Plans { get; private set; } = new List<SubscriptionPlan>();
        public IList<UserSubscription> Subscriptions { get; private set; } = new List<UserSubscription>();
        public PlanInput PlanForm { get; private set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public class PlanInput
        {
            public int PlanId { get; set; }

            [Required]
            [StringLength(100)]
            public string PlanName { get; set; } = string.Empty;

            [Range(typeof(decimal), "0", "10000")]
            public decimal Price { get; set; }

            [Required]
            [RegularExpression("^(Monthly|Yearly)$")]
            public string Duration { get; set; } = "Monthly";

            [StringLength(500)]
            public string? Description { get; set; } = string.Empty;

            [Required]
            [RegularExpression("^(Active|Inactive)$")]
            public string Status { get; set; } = "Active";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var access = SessionAuthorization.RequireAdmin(this);
            if (access is not null)
            {
                return access;
            }

            await LoadListsAsync();

            if (PlanId.HasValue)
            {
                var plan = await _context.SubscriptionPlans.FindAsync(PlanId.Value);
                if (plan is null)
                {
                    return NotFound();
                }

                PlanForm = new PlanInput
                {
                    PlanId = plan.PlanId,
                    PlanName = plan.PlanName,
                    Price = plan.Price,
                    Duration = plan.Duration,
                    Description = plan.Description,
                    Status = plan.Status
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSavePlanAsync(
            [Bind(Prefix = "PlanForm")] PlanInput form)
        {
            var access = SessionAuthorization.RequireAdmin(this);
            if (access is not null)
            {
                return access;
            }

            if (!ModelState.IsValid)
            {
                PlanForm = form;
                await LoadListsAsync();
                return Page();
            }

            SubscriptionPlan plan;
            if (form.PlanId > 0)
            {
                var existingPlan = await _context.SubscriptionPlans.FindAsync(form.PlanId);
                if (existingPlan is null)
                {
                    return NotFound();
                }

                plan = existingPlan;
            }
            else
            {
                plan = new SubscriptionPlan();
                _context.SubscriptionPlans.Add(plan);
            }

            plan.PlanName = form.PlanName.Trim();
            plan.Price = form.Price;
            plan.Duration = form.Duration;
            plan.Description = form.Description?.Trim() ?? string.Empty;
            plan.Status = form.Status;

            await _context.SaveChangesAsync();
            StatusMessage = "Subscription plan saved.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeletePlanAsync(int id)
        {
            var access = SessionAuthorization.RequireAdmin(this);
            if (access is not null)
            {
                return access;
            }

            var plan = await _context.SubscriptionPlans.FindAsync(id);
            if (plan is null)
            {
                return NotFound();
            }

            if (await _context.UserSubscriptions.AnyAsync(subscription => subscription.PlanId == id))
            {
                StatusMessage = "This plan has subscription history and cannot be deleted. Set it to Inactive instead.";
                return RedirectToPage();
            }

            _context.SubscriptionPlans.Remove(plan);
            await _context.SaveChangesAsync();
            StatusMessage = "Subscription plan deleted.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCancelSubscriptionAsync(int id)
        {
            var access = SessionAuthorization.RequireAdmin(this);
            if (access is not null)
            {
                return access;
            }

            var subscription = await _context.UserSubscriptions.FindAsync(id);
            if (subscription is null)
            {
                return NotFound();
            }

            if (subscription.Status == "Active")
            {
                subscription.Status = "Cancelled";
                await _context.SaveChangesAsync();
                StatusMessage = "Subscription cancelled.";
            }

            return RedirectToPage();
        }

        private async Task LoadListsAsync()
        {
            var plans = await _context.SubscriptionPlans
                .ToListAsync();
            Plans = plans
                .OrderBy(plan => plan.Price)
                .ThenBy(plan => plan.PlanName)
                .ToList();

            Subscriptions = await _context.UserSubscriptions
                .Include(subscription => subscription.User)
                .Include(subscription => subscription.Plan)
                .OrderByDescending(subscription => subscription.StartDate)
                .Take(100)
                .ToListAsync();
        }
    }
}
