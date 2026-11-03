using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;
using Muzik67.Models;

namespace Muzik67.Pages.Student
{
    public class GameSelectionModel : PageModel
    {
         readonly MuzikDbContext _context;
        public GameSelectionModel(MuzikDbContext context)
        {
            _context = context;
        }

        /* Pending for DB */
        public IList<Game> Games { get; set; } = new List<Game>();

        [BindProperty(SupportsGet = true, Name = "difficulty")]
        public string SelectedDifficulty { get; set; } = "All";

        public async Task<IActionResult> OnGetAsync(string difficulty)
        {
            SelectedDifficulty = string.IsNullOrEmpty(difficulty) ? "All" : difficulty;

            var userId = HttpContext.Session.GetInt32("UserId");
            var subscription = userId.HasValue
                ? await SubscriptionAccess.GetActiveAsync(_context, userId.Value)
                : null;
            var hasPremiumAccess = subscription?.Plan?.Price > 0;

            var query = _context.Games
                .Where(game => game.Status == "Active"
                    || hasPremiumAccess && game.Status == "Premium");

            if (SelectedDifficulty != "All")
            {
                query = query.Where(g => g.DifficultyLevel == SelectedDifficulty);
            }

            Games = await query.ToListAsync();
            
            return Page();
        }
    }
}
