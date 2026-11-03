using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;

namespace Muzik67.Pages.Student;

public class ProgressModel : PageModel
{
    private readonly MuzikDbContext _context;

    public ProgressModel(MuzikDbContext context)
    {
        _context = context;
    }

    public string Username { get; private set; } = string.Empty;

    public IReadOnlyList<GameProgressSummary> GameProgress { get; private set; } =
        Array.Empty<GameProgressSummary>();

    public IList<GameResultItem> RecentResults { get; private set; } = new List<GameResultItem>();

    // Summary metrics
    public int CompletedGames => GameProgress.Count;
    public int TotalAttempts => GameProgress.Sum(progress => progress.Attempts);
    public int AverageScore => GameProgress.Count == 0
        ? 0
        : (int)Math.Round(GameProgress.Average(progress => progress.ScorePercentage));

    public class GameResultItem
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

        var progressRecords = await _context.GameProgresses
            .AsNoTracking()
            .Where(progress => progress.UserId == userId.Value &&
                               progress.Game != null &&
                               progress.Game.Status == "Active")
            .Include(progress => progress.Game)
                .ThenInclude(game => game!.Quizzes)
            .OrderByDescending(progress => progress.DateOfCompletion)
            .ToListAsync();

        // Grouped summaries for per-game progress bars
        var records = progressRecords
            .GroupBy(progress => new
            {
                progress.GameId,
                GameName = progress.Game?.GameName ?? "Unknown game",
                DifficultyLevel = progress.Game?.DifficultyLevel ?? "Unknown"
            })
            .Select(group => new GameProgressSummary
            {
                GameId = group.Key.GameId,
                GameName = group.Key.GameName,
                Difficulty = group.Key.DifficultyLevel,
                HighestScore = group.Max(progress => progress.Score),
                Attempts = group.Sum(progress => progress.Attempts),
                LastCompleted = group.Max(progress => progress.DateOfCompletion),
                TotalQuestions = group
                    .SelectMany(progress => progress.Game?.Quizzes ?? [])
                    .Count()
            })
            .OrderByDescending(progress => progress.LastCompleted)
            .ToList();

        foreach (var progress in records)
        {
            progress.TotalQuestions = Math.Max(progress.TotalQuestions, 1);
        }

        GameProgress = records;

        // Recent attempts for the detailed history log
        RecentResults = progressRecords
            .Take(10)
            .Select(progress => new GameResultItem
            {
                GameName = progress.Game?.GameName ?? "Game unavailable",
                Score = progress.Score,
                Attempts = progress.Attempts,
                CompletedAt = progress.DateOfCompletion
            })
            .ToList();

        return Page();
    }
}

// Summary data displayed for each completed game
public sealed class GameProgressSummary
{
    public int GameId { get; init; }
    public string GameName { get; init; } = string.Empty;
    public string Difficulty { get; init; } = string.Empty;
    public int HighestScore { get; init; }
    public int Attempts { get; init; }
    public DateTime LastCompleted { get; init; }
    public int TotalQuestions { get; set; }

    // Converts the highest score into a percentage for the progress bar
    public int ScorePercentage => Math.Clamp(
        (int)Math.Round(HighestScore * 100.0 / TotalQuestions),
        0,
        100);
}
