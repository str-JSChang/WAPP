using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;
using Muzik67.Models;
using Muzik67.Pages.Shared;

namespace Muzik67.Pages.Student
{
    public class GameSelectionModel : SessionPageModel
    {
         readonly MuzikDbContext _context;
        public GameSelectionModel(MuzikDbContext context)
        {
            _context = context;
        }

        /* Pending for DB */
        public IList<Game> Games { get; set; } = new List<Game>();
        protected override bool MemberOnly => true;

        [BindProperty(SupportsGet = true, Name = "difficulty")]
        public string SelectedDifficulty { get; set; } = "All";

        public async Task<IActionResult> OnGetAsync(string? difficulty)
        {
            var allowedDifficulties = new[] { "All", "Easy", "Medium", "Hard" };
            SelectedDifficulty = allowedDifficulties.Contains(difficulty, StringComparer.OrdinalIgnoreCase)
                ? allowedDifficulties.First(item => string.Equals(item, difficulty, StringComparison.OrdinalIgnoreCase))
                : "All";

            var subscription = await SubscriptionAccess.GetActiveAsync(_context, CurrentUserId);
            var hasPremiumAccess = subscription?.Plan?.Price > 0;

            var query = _context.Games
                .Where(game => game.ApprovalStatus == "Approved" && game.Quizzes.Any() && (game.Status == "Active"
                    || hasPremiumAccess && game.Status == "Premium"));

            if (SelectedDifficulty != "All")
            {
                query = query.Where(g => g.DifficultyLevel == SelectedDifficulty);
            }

            Games = await query.OrderBy(g => g.SortOrder).ThenBy(g => g.DifficultyLevel).ThenBy(g => g.GameName).ToListAsync();
            
            return Page();
        }
    }
}
