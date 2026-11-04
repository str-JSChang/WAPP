using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzik67.Models;
using Muzik67.Pages.Shared;

namespace Muzik67.Pages.Admin;

public class SubscriptionsModel : SessionPageModel
{
    private readonly ModuleDataService _data;
    private readonly ILogger<SubscriptionsModel> _logger;

    public SubscriptionsModel(ModuleDataService data, ILogger<SubscriptionsModel> logger)
    {
        _data = data;
        _logger = logger;
    }

    protected override bool AdminOnly => true;

    [BindProperty]
    public PlanInput Input { get; set; } = new();

    public List<SubscriptionPlanView> Plans { get; private set; } = [];
    public List<MemberSubscription> MemberSubscriptions { get; private set; } = [];

    [TempData]
    public string? StatusMessage { get; set; }

    public string? ErrorMessage { get; private set; }
    public bool ShowPlanForm { get; private set; }

    public async Task OnGetAsync(int? editId, bool add = false)
    {
        await LoadPageDataAsync();
        ShowPlanForm = add || editId.HasValue;

        if (editId is null)
        {
            return;
        }

        var plan = Plans.FirstOrDefault(item => item.PlanId == editId.Value);
        if (plan is null)
        {
            ErrorMessage ??= "That subscription plan could not be found.";
            return;
        }

        Input = new PlanInput
        {
            PlanId = plan.PlanId,
            PlanName = plan.PlanName,
            Price = plan.Price,
            Duration = plan.Duration,
            Description = plan.Description,
            Status = plan.Status
        };
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!ModelState.IsValid)
        {
            ShowPlanForm = true;
            await LoadPageDataAsync();
            return Page();
        }

        var plan = new Muzik67.Models.SubscriptionPlan
        {
            PlanId = Input.PlanId,
            PlanName = Input.PlanName.Trim(),
            Price = Input.Price,
            Duration = Input.Duration,
            Description = Input.Description.Trim(),
            Status = Input.Status
        };

        try
        {
            if (plan.PlanId == 0)
            {
                await _data.AddSubscriptionPlanAsync(plan);
                StatusMessage = "Subscription plan created.";
            }
            else if (await _data.UpdateSubscriptionPlanAsync(plan))
            {
                StatusMessage = "Subscription plan updated.";
            }
            else
            {
                ErrorMessage = "That subscription plan could not be found.";
                await LoadPageDataAsync();
                return Page();
            }

            return RedirectToPage();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Could not save subscription plan {PlanId}", plan.PlanId);
            ErrorMessage = "We couldn't save the plan. Please check the plan details and try again.";
            ShowPlanForm = true;
            await LoadPageDataAsync();
            return Page();
        }
        catch (DbException exception)
        {
            _logger.LogError(exception, "Could not save subscription plan {PlanId}", plan.PlanId);
            ErrorMessage = "We couldn't save the plan. Please check the database connection and try again.";
            ShowPlanForm = true;
            await LoadPageDataAsync();
            return Page();
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogError(exception, "Could not save subscription plan {PlanId}", plan.PlanId);
            ErrorMessage = "We couldn't save the plan. Please try again later.";
            ShowPlanForm = true;
            return Page();
        }
    }

    public async Task<IActionResult> OnPostDeleteAsync(int planId)
    {
        if (planId <= 0)
        {
            return BadRequest();
        }

        try
        {
            var result = await _data.DeleteSubscriptionPlanAsync(planId);
            StatusMessage = result switch
            {
                DeletePlanResult.Deleted => "Subscription plan deleted.",
                DeletePlanResult.HasSubscriptions =>
                    "This plan cannot be deleted because subscription records use it.",
                _ => "That subscription plan could not be found."
            };
        }
        catch (DbException exception)
        {
            _logger.LogError(exception, "Could not delete subscription plan {PlanId}", planId);
            ErrorMessage = "We couldn't delete the plan. Please try again later.";
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogError(exception, "Could not delete subscription plan {PlanId}", planId);
            ErrorMessage = "We couldn't delete the plan. Please try again later.";
        }

        await LoadPageDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostCancelAsync(int subscriptionId)
    {
        StatusMessage = await _data.CancelSubscriptionAsync(subscriptionId)
            ? "Subscription cancelled."
            : "That subscription could not be found.";
        return RedirectToPage();
    }

    private async Task LoadPageDataAsync()
    {
        try
        {
            Plans = await _data.GetSubscriptionPlansAsync();
            MemberSubscriptions = await _data.GetMemberSubscriptionsAsync();
        }
        catch (DbException exception)
        {
            _logger.LogError(exception, "Could not load subscription management data");
            ErrorMessage ??= "Subscription data is unavailable. Please check the database connection.";
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogError(exception, "Could not load subscription management data");
            ErrorMessage ??= "Subscription data is unavailable. Please try again later.";
        }
    }

    public sealed class PlanInput
    {
        public int PlanId { get; set; }

        [Required]
        [StringLength(80, MinimumLength = 2)]
        public string PlanName { get; set; } = string.Empty;

        [Range(typeof(decimal), "0", "99999.99")]
        public decimal Price { get; set; }

        [Required]
        [RegularExpression("^(Monthly|Yearly)$", ErrorMessage = "Choose Monthly or Yearly.")]
        public string Duration { get; set; } = "Monthly";

        [Required]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^(Active|Inactive)$", ErrorMessage = "Choose Active or Inactive.")]
        public string Status { get; set; } = "Active";
    }
}
