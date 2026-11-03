using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Models;

namespace Muzik67.Helpers
{
    public static class SubscriptionAccess
    {
        public static async Task<UserSubscription?> GetActiveAsync(MuzikDbContext context, int userId)
        {
            var now = DateTime.UtcNow;

            return await context.UserSubscriptions
                .Include(subscription => subscription.Plan)
                .Where(subscription => subscription.UserId == userId
                    && subscription.Status == "Active"
                    && subscription.EndDate > now)
                .OrderByDescending(subscription => subscription.StartDate)
                .FirstOrDefaultAsync();
        }

        public static bool CanAccess(string status, UserSubscription? subscription)
        {
            return status == "Active"
                || status == "Premium" && subscription?.Plan?.Price > 0;
        }
    }
}
