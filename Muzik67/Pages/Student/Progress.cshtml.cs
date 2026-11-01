using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;

namespace Muzik67.Pages.Student;

public class ProgressModel : PageModel
{
    // Database context used to retrieve the student's game progress
    private readonly MuzikDbContext _context;

    public ProgressModel(MuzikDbContext context)
    {
        _context = context;
    }

    public IReadOnlyList<GameProgressSummary> GameProgress { get; private set; } =
        Array.Empty<GameProgressSummary>();

    // Summary values displayed at the top of the progress page
    public int CompletedGames => GameProgress.Count;
    public int TotalAttempts => GameProgress.Sum(progress => progress.Attempts);
    public int AverageScore => GameProgress.Count == 0
        ? 0
        : (int)Math.Round(GameProgress.Average(progress => progress.ScorePercentage));

    // Retrieves and groups progress records for the logged-in student
    public async Task<IActionResult> OnGetAsync()
    {
        if (!TryGetUserId(out var userId))
        {
            return RedirectToPage("/Account/Login", new { returnUrl = Request.Path });
        }

        var progressRecords = await _context.GameProgresses
            .AsNoTracking()
            .Where(progress => progress.UserId == userId &&
                               progress.Game != null &&
                               progress.Game.Status == "Active")
            .Include(progress => progress.Game)
                .ThenInclude(game => game!.Quizzes)
            .ToListAsync();

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
        return Page();
    }

    // Gets the current student ID from the login session
    private bool TryGetUserId(out int userId)
    {
        userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        return userId > 0;
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
