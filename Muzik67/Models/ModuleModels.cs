namespace Muzik67.Models;

public sealed record UserProfile(
    int UserId,
    string Username,
    string Email,
    string Role,
    DateTime RegistrationDate);

public sealed class ProfileInput
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.StringLength(50, MinimumLength = 3)]
    public string Username { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string Email { get; set; } = string.Empty;
}

public sealed record AdminDashboardSummary(
    int TotalUsers,
    int TotalPlans,
    int ActiveSubscriptions);

public sealed record SubscriptionPlanView(
    int PlanId,
    string PlanName,
    decimal Price,
    string Duration,
    string Description,
    int ActiveSubscribers);

public sealed record MemberSubscription(
    string Username,
    string PlanName,
    string Status,
    DateTime StartDate,
    DateTime? EndDate);

public enum DeletePlanResult
{
    Deleted,
    NotFound,
    HasSubscriptions
}
