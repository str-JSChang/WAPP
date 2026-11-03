using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;
using Muzik67.Models;

namespace Muzik67.Pages.Student;

public class GameDetailModel : PageModel
{
    private readonly MuzikDbContext _context;

    public GameDetailModel(MuzikDbContext context)
    {
        _context = context;
    }

    public Game? Game { get; private set; }
    public IReadOnlyList<GameQuiz> Questions { get; private set; } = Array.Empty<GameQuiz>();
    public GameQuiz? CurrentQuestion => Questions.ElementAtOrDefault(QuestionNumber - 1);
    public IReadOnlyList<string> AnswerOptions => ParseOptions(CurrentQuestion?.AnswerOptions);

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public int QuestionNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int Score { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Lives { get; set; } = 3;

    [BindProperty]
    public string? SelectedAnswer { get; set; }

    [TempData]
    public string? FeedbackMessage { get; set; }

    [TempData]
    public bool FeedbackWasCorrect { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var access = SessionAuthorization.RequireMember(this);
        if (access is not null)
        {
            return access;
        }

        if (!await LoadGameAsync())
        {
            return RedirectToPage("/Student/GameSelection");
        }

        QuestionNumber = Math.Clamp(QuestionNumber, 1, Math.Max(Questions.Count, 1));
        Score = Math.Clamp(Score, 0, Questions.Count);
        Lives = Math.Clamp(Lives, 0, 3);
        return Page();
    }

    public async Task<IActionResult> OnPostAnswerAsync()
    {
        var access = SessionAuthorization.RequireMember(this);
        if (access is not null)
        {
            return access;
        }

        if (!await LoadGameAsync() || CurrentQuestion is null)
        {
            return RedirectToPage("/Student/GameSelection");
        }

        if (string.IsNullOrWhiteSpace(SelectedAnswer))
        {
            ModelState.AddModelError(nameof(SelectedAnswer), "Choose one answer before checking.");
            return Page();
        }

        var correct = string.Equals(
            SelectedAnswer.Trim(),
            CurrentQuestion.CorrectAnswer.Trim(),
            StringComparison.OrdinalIgnoreCase);

        if (correct)
        {
            Score++;
            FeedbackMessage = "+20 XP — correct answer.";
            FeedbackWasCorrect = true;
        }
        else
        {
            Lives = Math.Max(0, Lives - 1);
            FeedbackMessage = $"Not quite. The correct answer was {CurrentQuestion.CorrectAnswer}.";
            FeedbackWasCorrect = false;
        }

        return ContinueOrFinish();
    }

    public async Task<IActionResult> OnPostSkipAsync()
    {
        var access = SessionAuthorization.RequireMember(this);
        if (access is not null)
        {
            return access;
        }

        if (!await LoadGameAsync())
        {
            return RedirectToPage("/Student/GameSelection");
        }

        FeedbackMessage = "Question skipped. You can replay the quiz later.";
        FeedbackWasCorrect = false;
        return ContinueOrFinish();
    }

    private IActionResult ContinueOrFinish()
    {
        if (QuestionNumber >= Questions.Count || Lives <= 0)
        {
            return RedirectToPage("/Student/GameResult", new
            {
                GameId = Id,
                Score,
                Attempts = 1
            });
        }

        return RedirectToPage(new
        {
            id = Id,
            QuestionNumber = QuestionNumber + 1,
            Score,
            Lives
        });
    }

    private async Task<bool> LoadGameAsync()
    {
        if (Id <= 0)
        {
            return false;
        }

        Game = await _context.Games
            .AsNoTracking()
            .Include(game => game.Quizzes)
            .FirstOrDefaultAsync(game => game.GameId == Id);

        if (Game is null)
        {
            return false;
        }

        var userId = HttpContext.Session.GetInt32("UserId");
        var subscription = userId.HasValue
            ? await SubscriptionAccess.GetActiveAsync(_context, userId.Value)
            : null;

        if (!SubscriptionAccess.CanAccess(Game.Status, subscription))
        {
            return false;
        }

        Questions = Game.Quizzes.OrderBy(question => question.QuestionId).ToList();
        return Questions.Count > 0;
    }

    private static IReadOnlyList<string> ParseOptions(string? answerOptions)
    {
        if (string.IsNullOrWhiteSpace(answerOptions))
        {
            return Array.Empty<string>();
        }

        if (answerOptions.TrimStart().StartsWith('['))
        {
            try
            {
                return JsonSerializer.Deserialize<string[]>(answerOptions) ?? Array.Empty<string>();
            }
            catch (JsonException)
            {
                // Fall back to the comma-separated format used by the current admin form.
            }
        }

        return answerOptions
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(option => option.Length > 0)
            .ToArray();
    }
}
