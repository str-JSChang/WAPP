using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Pages.Shared;

namespace Muzik67.Pages.Student;

public class ProgressModel : SessionPageModel
{
    private readonly MuzikDbContext _context;

    public ProgressModel(MuzikDbContext context) => _context = context;

    protected override bool MemberOnly => true;

    public string Username { get; private set; } = string.Empty;
    [BindProperty(SupportsGet = true)]
    public int? SelectedGameId { get; set; }

    public IReadOnlyList<GameFilterOption> AvailableGames { get; private set; } = [];
    public IReadOnlyList<GameProgressSummary> GameProgress { get; private set; } = [];
    public IList<GameResultItem> RecentResults { get; private set; } = [];
    public IList<LessonProgressItem> LessonProgress { get; private set; } = [];
    public IList<BadgeItem> Badges { get; private set; } = [];

    public int CompletedGames => GameProgress.Count;
    public int TotalAttempts => GameProgress.Sum(progress => progress.Attempts);
    public int AverageScore => GameProgress.Count == 0
        ? 0
        : (int)Math.Round(GameProgress.Average(progress => progress.ScorePercentage));

    public async Task<IActionResult> OnGetAsync()
    {
        Username = HttpContext.Session.GetString("Username") ?? "Member";

        AvailableGames = await _context.Games
            .AsNoTracking()
            .Where(game => _context.GameProgresses.Any(progress =>
                progress.UserId == CurrentUserId && progress.GameId == game.GameId))
            .OrderBy(game => game.SortOrder)
            .ThenBy(game => game.GameName)
            .Select(game => new GameFilterOption
            {
                GameId = game.GameId,
                GameName = game.GameName
            })
            .ToListAsync();

        var progressQuery = _context.GameProgresses
            .AsNoTracking()
            .Where(progress => progress.UserId == CurrentUserId && progress.Game != null)
            .Include(progress => progress.Game)
                .ThenInclude(game => game!.Quizzes)
            .AsQueryable();

        if (SelectedGameId.HasValue)
        {
            progressQuery = progressQuery.Where(progress =>
                progress.GameId == SelectedGameId.Value);
        }

        var progressRecords = await progressQuery
            .OrderByDescending(progress => progress.DateOfCompletion)
            .ToListAsync();

        GameProgress = progressRecords
            .GroupBy(progress => new
            {
                progress.GameId,
                GameName = progress.Game?.GameName ?? "Unknown game",
                DifficultyLevel = progress.Game?.DifficultyLevel ?? "Unknown",
                TotalQuestions = progress.Game?.Quizzes.Count ?? 0
            })
            .Select(group => new GameProgressSummary
            {
                GameId = group.Key.GameId,
                GameName = group.Key.GameName,
                Difficulty = group.Key.DifficultyLevel,
                HighestScore = group.Max(progress => progress.Score),
                Attempts = group.Count(),
                LastCompleted = group.Max(progress => progress.DateOfCompletion),
                TotalQuestions = Math.Max(group.Key.TotalQuestions, 1)
            })
            .OrderByDescending(progress => progress.LastCompleted)
            .ToList();

        RecentResults = progressRecords.Take(10).Select(progress => new GameResultItem
        {
            GameName = progress.Game?.GameName ?? "Game unavailable",
            Score = progress.Score,
            TotalQuestions = progress.Game?.Quizzes.Count ?? 0,
            CompletedAt = progress.DateOfCompletion
        }).ToList();

        LessonProgress = await _context.LessonProgresses
            .AsNoTracking()
            .Where(progress => progress.UserId == CurrentUserId)
            .Include(progress => progress.Content)
            .OrderByDescending(progress => progress.CompletedAt)
            .Select(progress => new LessonProgressItem
            {
                ContentId = progress.ContentId,
                LessonTitle = progress.Content != null ? progress.Content.Title : "Lesson unavailable",
                CompletedAt = progress.CompletedAt
            })
            .ToListAsync();

        Badges = await _context.UserBadges
            .AsNoTracking()
            .Where(badge => badge.UserId == CurrentUserId)
            .OrderByDescending(badge => badge.EarnedAt)
            .Select(badge => new BadgeItem
            {
                Name = badge.BadgeName,
                EarnedAt = badge.EarnedAt
            })
            .ToListAsync();

        return Page();
    }

    public sealed class GameResultItem
    {
        public string GameName { get; init; } = string.Empty;
        public int Score { get; init; }
        public int TotalQuestions { get; init; }
        public DateTime CompletedAt { get; init; }
    }

    public sealed class GameFilterOption
    {
        public int GameId { get; init; }
        public string GameName { get; init; } = string.Empty;
    }

    public sealed class LessonProgressItem
    {
        public int ContentId { get; init; }
        public string LessonTitle { get; init; } = string.Empty;
        public DateTime CompletedAt { get; init; }
    }

    public sealed class BadgeItem
    {
        public string Name { get; init; } = string.Empty;
        public DateTime EarnedAt { get; init; }
    }
}

public sealed class GameProgressSummary
{
    public int GameId { get; init; }
    public string GameName { get; init; } = string.Empty;
    public string Difficulty { get; init; } = string.Empty;
    public int HighestScore { get; init; }
    public int Attempts { get; init; }
    public DateTime LastCompleted { get; init; }
    public int TotalQuestions { get; init; }
    public int ScorePercentage => Math.Clamp(
        (int)Math.Round(HighestScore * 100.0 / TotalQuestions), 0, 100);
}
