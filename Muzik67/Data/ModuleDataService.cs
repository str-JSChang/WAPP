using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Models;

namespace Muzik67;

public sealed class ModuleDataService
{
    private readonly MuzikDbContext _context;

    public ModuleDataService(MuzikDbContext context)
    {
        _context = context;
    }

    public Task<UserProfile?> GetProfileAsync(int userId)
    {
        return _context.Users
            .AsNoTracking()
            .Where(user => user.UserId == userId)
            .Select(user => new UserProfile(
                user.UserId,
                user.Username,
                user.Email,
                user.Role,
                user.RegistrationDate))
            .SingleOrDefaultAsync();
    }

    public async Task<bool> UpdateProfileAsync(int userId, string username, string email)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user is null)
        {
            return false;
        }

        user.Username = username;
        user.Email = email;
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<bool> IsUsernameInUseAsync(int userId, string username) =>
        _context.Users.AnyAsync(user => user.UserId != userId && user.Username.ToLower() == username.ToLower());

    public Task<bool> IsEmailInUseAsync(int userId, string email) =>
        _context.Users.AnyAsync(user => user.UserId != userId && user.Email.ToLower() == email.ToLower());

    public async Task<AdminDashboardSummary> GetAdminDashboardSummaryAsync()
    {
        var today = DateTime.UtcNow.Date;
        var totalUsers = await _context.Users.CountAsync();
        var totalPlans = await _context.SubscriptionPlans.CountAsync();
        var activeSubscriptions = await _context.UserSubscriptions.CountAsync(subscription =>
            subscription.Status == "Active" &&
            subscription.StartDate <= today &&
            (subscription.EndDate == default || subscription.EndDate >= today));

        return new AdminDashboardSummary(totalUsers, totalPlans, activeSubscriptions);
    }

    public async Task<List<SubscriptionPlanView>> GetSubscriptionPlansAsync()
    {
        var plans = await _context.SubscriptionPlans
            .AsNoTracking()
            .ToListAsync();

        var today = DateTime.UtcNow.Date;
        var subscriberCounts = await _context.UserSubscriptions
            .AsNoTracking()
            .Where(subscription =>
                subscription.Status == "Active" &&
                subscription.StartDate <= today &&
                (subscription.EndDate == default || subscription.EndDate >= today))
            .GroupBy(subscription => subscription.PlanId)
            .Select(group => new
            {
                PlanId = group.Key,
                Count = group.Select(subscription => subscription.UserId).Distinct().Count()
            })
            .ToDictionaryAsync(group => group.PlanId, group => group.Count);

        return plans
            .OrderBy(plan => plan.Price)
            .ThenBy(plan => plan.PlanName)
            .Select(plan => new SubscriptionPlanView(
                plan.PlanId,
                plan.PlanName,
                plan.Price,
                plan.Duration,
                plan.Description,
                plan.Status,
                subscriberCounts.GetValueOrDefault(plan.PlanId)))
            .ToList();
    }

    public async Task<List<MemberSubscription>> GetMemberSubscriptionsAsync()
    {
        var subscriptions = await _context.UserSubscriptions
            .AsNoTracking()
            .Include(subscription => subscription.User)
            .Include(subscription => subscription.Plan)
            .OrderByDescending(subscription => subscription.StartDate)
            .Take(100)
            .ToListAsync();

        var today = DateTime.UtcNow.Date;

        return subscriptions
            .Where(subscription => subscription.User is not null && subscription.Plan is not null)
            .Select(subscription =>
            {
                var status = subscription.Status;
                if (status == "Active" && subscription.StartDate.Date > today)
                {
                    status = "Scheduled";
                }
                else if (status == "Active" &&
                         subscription.EndDate != default &&
                         subscription.EndDate.Date < today)
                {
                    status = "Expired";
                }

                return new MemberSubscription(
                    subscription.SubscriptionId,
                    subscription.User!.Username,
                    subscription.Plan!.PlanName,
                    status,
                    subscription.StartDate,
                    subscription.EndDate == default ? null : subscription.EndDate);
            })
            .ToList();
    }

    public async Task AddSubscriptionPlanAsync(Models.SubscriptionPlan plan)
    {
        _context.SubscriptionPlans.Add(plan);
        await _context.SaveChangesAsync();
    }

    public Task<bool> IsSubscriptionPlanNameInUseAsync(int planId, string planName) =>
        _context.SubscriptionPlans.AnyAsync(plan =>
            plan.PlanId != planId && plan.PlanName.ToLower() == planName.ToLower());

    public async Task<bool> UpdateSubscriptionPlanAsync(Models.SubscriptionPlan plan)
    {
        var existingPlan = await _context.SubscriptionPlans.FindAsync(plan.PlanId);
        if (existingPlan is null)
        {
            return false;
        }

        existingPlan.PlanName = plan.PlanName;
        existingPlan.Price = plan.Price;
        existingPlan.Duration = plan.Duration;
        existingPlan.Description = plan.Description;
        existingPlan.Status = plan.Status;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CancelSubscriptionAsync(int subscriptionId)
    {
        var subscription = await _context.UserSubscriptions.FindAsync(subscriptionId);
        if (subscription is null)
        {
            return false;
        }

        if (subscription.Status == "Active")
        {
            subscription.Status = "Cancelled";
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<DeletePlanResult> DeleteSubscriptionPlanAsync(int planId)
    {
        var deletedCount = await _context.SubscriptionPlans
            .Where(plan =>
                plan.PlanId == planId &&
                !_context.UserSubscriptions.Any(subscription => subscription.PlanId == planId))
            .ExecuteDeleteAsync();

        if (deletedCount > 0)
        {
            return DeletePlanResult.Deleted;
        }

        var planExists = await _context.SubscriptionPlans
            .AnyAsync(plan => plan.PlanId == planId);

        if (!planExists)
        {
            return DeletePlanResult.NotFound;
        }

        return DeletePlanResult.HasSubscriptions;
    }
}
