using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Models;

namespace Muzik67.Pages.Student;

public class GameResultModel : PageModel
{
    // Database and logging services used by the game result page
    private readonly MuzikDbContext _context;
    private readonly ILogger<GameResultModel> _logger;

    public GameResultModel(MuzikDbContext context, ILogger<GameResultModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    public Game? Game { get; private set; }
    public int TotalQuestions { get; private set; }
    public string? ErrorMessage { get; private set; }

    // Converts the student's score into a percentage for the result display
    public int ScorePercentage => TotalQuestions == 0
        ? 0
        : (int)Math.Round(Score * 100.0 / TotalQuestions);

    // Values received from the completed game
    [BindProperty(SupportsGet = true)]
    public int GameId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Score { get; set; }

    [BindProperty]
    public int Attempts { get; set; } = 1;

    [BindProperty]
    public bool SaveResult { get; set; }

    // Loads the selected game result for the logged-in student
    public async Task<IActionResult> OnGetAsync()
    {
        if (!TryGetUserId(out _))
        {
            return RedirectToPage("/Account/Login", new { returnUrl = Request.Path + Request.QueryString });
        }

        if (!await LoadGameAsync())
        {
            ErrorMessage = "We could not find that game. Please choose a game and try again.";
            return Page();
        }

        if (Score < 0 || Score > TotalQuestions)
        {
            ErrorMessage = $"That score is not valid. Enter a score from 0 to {TotalQuestions}.";
            return Page();
        }

        Attempts = Math.Max(Attempts, 1);
        return Page();
    }

    // Validates and saves the completed game result
    public async Task<IActionResult> OnPostAsync()
    {
        if (!TryGetUserId(out var userId))
        {
            return RedirectToPage("/Account/Login", new { returnUrl = Request.Path + Request.QueryString });
        }

        if (!await LoadGameAsync())
        {
            ErrorMessage = "We could not find that game. Please choose a game and try again.";
            return Page();
        }

        if (Score < 0 || Score > TotalQuestions)
        {
            ModelState.AddModelError(nameof(Score), $"Score must be between 0 and {TotalQuestions}.");
        }

        if (Attempts < 1)
        {
            ModelState.AddModelError(nameof(Attempts), "Attempts must be at least 1.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (SaveResult)
        {
            var progress = new GameProgress
            {
                UserId = userId,
                GameId = GameId,
                Attempts = Attempts,
                Score = Score,
                DateOfCompletion = DateTime.UtcNow
            };

            _context.GameProgresses.Add(progress);
            await _context.SaveChangesAsync();
            _logger.LogInformation(
                "Saved game progress for user {UserId}, game {GameId}, score {Score}.",
                userId,
                GameId,
                Score);
        }

        return RedirectToPage(new { GameId, Score, saved = SaveResult });
    }

    // Loads the active game and counts its questions
    private async Task<bool> LoadGameAsync()
    {
        if (GameId <= 0)
        {
            return false;
        }

        Game = await _context.Games
            .Include(game => game.Quizzes)
            .AsNoTracking()
            .FirstOrDefaultAsync(game => game.GameId == GameId && game.Status == "Active");

        if (Game is null)
        {
            return false;
        }

        TotalQuestions = Game.Quizzes.Count;
        return true;
    }

    // Gets the current student ID from the login session
    private bool TryGetUserId(out int userId)
    {
        userId = HttpContext.Session.GetInt32("UserId") ?? 0;
        return userId > 0;
    }
}
