using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;

namespace Muzik67.Pages.Student
{
    public class ProgressModel : PageModel
    {
        private readonly MuzikDbContext _context;

        public ProgressModel(MuzikDbContext context)
        {
            _context = context;
        }

        public string Username { get; private set; } = string.Empty;
        public int ResultCount { get; private set; }
        public int TotalAttempts { get; private set; }
        public double? AverageScore { get; private set; }
        public IList<GameResult> RecentResults { get; private set; } = new List<GameResult>();

        public class GameResult
        {
            public string GameName { get; init; } = string.Empty;
            public int Score { get; init; }
            public int Attempts { get; init; }
            public DateTime CompletedAt { get; init; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var access = SessionAuthorization.RequireMember(this);
            if (access is not null)
            {
                return access;
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null)
            {
                return RedirectToPage("/Account/Login");
            }

            Username = HttpContext.Session.GetString("Username") ?? "Member";

            var results = await _context.GameProgresses
                .Where(progress => progress.UserId == userId.Value)
                .OrderByDescending(progress => progress.DateOfCompletion)
                .Select(progress => new GameResult
                {
                    GameName = progress.Game == null ? "Game unavailable" : progress.Game.GameName,
                    Score = progress.Score,
                    Attempts = progress.Attempts,
                    CompletedAt = progress.DateOfCompletion
                })
                .ToListAsync();

            ResultCount = results.Count;
            TotalAttempts = results.Sum(result => result.Attempts);
            AverageScore = results.Count == 0 ? null : results.Average(result => result.Score);
            RecentResults = results.Take(10).ToList();

            return Page();
        }
    }
}
