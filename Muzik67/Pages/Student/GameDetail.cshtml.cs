using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;
using Muzik67.Models;
using Muzik67.Pages.Shared;

namespace Muzik67.Pages.Student;

public class GameDetailModel : SessionPageModel
{
    private readonly MuzikDbContext _context;
    private const string StateKeyPrefix = "GameState:";

    public GameDetailModel(MuzikDbContext context) => _context = context;

    protected override bool MemberOnly => true;

    public Game? Game { get; private set; }
    public IReadOnlyList<GameQuiz> Questions { get; private set; } = [];
    public GameQuiz? CurrentQuestion => Questions.ElementAtOrDefault(QuestionNumber - 1);
    public IReadOnlyList<string> AnswerOptions => ParseOptions(CurrentQuestion?.AnswerOptions);
    public IReadOnlyList<string> AnswerOptionImages => ParseOptions(CurrentQuestion?.OptionImageUrls);
    public int QuestionNumber { get; private set; } = 1;
    public int Score { get; private set; }
    public int Lives { get; private set; } = 3;
    public double? AudioCueFrequency => GetAudioCueFrequency(CurrentQuestion);

    [BindProperty]
    public int Id { get; set; }

    [BindProperty]
    public string? SelectedAnswer { get; set; }

    [TempData]
    public string? FeedbackMessage { get; set; }

    [TempData]
    public bool FeedbackWasCorrect { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, bool restart = false)
    {
        Id = id;
        if (!await LoadGameAsync())
        {
            TempData["GameMessage"] = "That game is unavailable for your account.";
            return RedirectToPage("/Student/GameSelection");
        }

        var state = restart ? null : ReadState();
        if (state is null || state.GameId != Id || state.QuestionIndex < 0 || state.QuestionIndex >= Questions.Count)
        {
            state = new GameSessionState { GameId = Id };
            WriteState(state);
        }

        ApplyState(state);
        return Page();
    }

    public async Task<IActionResult> OnPostAnswerAsync()
    {
        if (!await LoadGameAsync())
        {
            return RedirectToPage("/Student/GameSelection");
        }

        var state = ReadState();
        if (!IsValidState(state))
        {
            TempData["GameMessage"] = "That game session expired. Please start the game again.";
            return RedirectToPage(new { id = Id, restart = true });
        }

        ApplyState(state!);
        var activityType = CurrentQuestion?.ActivityType ?? "MultipleChoice";
        var submittedAnswer = SelectedAnswer?.Trim() ?? string.Empty;
        var validAnswer = activityType == "Melody"
            ? IsValidMelody(submittedAnswer, AnswerOptions, CurrentQuestion!.CorrectAnswer)
            : AnswerOptions.Contains(submittedAnswer, StringComparer.OrdinalIgnoreCase);
        if (!validAnswer)
        {
            ModelState.AddModelError(nameof(SelectedAnswer), activityType == "Melody"
                ? "Play the complete melody before checking."
                : "Choose one of the available answers before checking.");
            return Page();
        }

        // SERVER CALCULATES THE SCORE because DevTools users be like infinite points go brrrrr
        var correct = activityType == "Melody"
            ? string.Equals(NormalizeMelody(submittedAnswer), NormalizeMelody(CurrentQuestion!.CorrectAnswer), StringComparison.OrdinalIgnoreCase)
            : string.Equals(submittedAnswer, CurrentQuestion!.CorrectAnswer.Trim(), StringComparison.OrdinalIgnoreCase);

        if (correct)
        {
            state!.Score++;
            FeedbackMessage = activityType == "Melody" ? "Perfect performance — you copied the melody!" : "Correct — great listening!";
            FeedbackWasCorrect = true;
        }
        else
        {
            state!.Lives = Math.Max(0, state.Lives - 1);
            FeedbackMessage = $"Not quite. The correct answer was {CurrentQuestion.CorrectAnswer}.";
            FeedbackWasCorrect = false;
        }

        return await AdvanceOrFinishAsync(state);
    }

    public async Task<IActionResult> OnPostSkipAsync()
    {
        if (!await LoadGameAsync())
        {
            return RedirectToPage("/Student/GameSelection");
        }

        var state = ReadState();
        if (!IsValidState(state))
        {
            TempData["GameMessage"] = "That game session expired. Please start the game again.";
            return RedirectToPage(new { id = Id, restart = true });
        }

        FeedbackMessage = "Question skipped. You can replay the quiz later.";
        FeedbackWasCorrect = false;
        return await AdvanceOrFinishAsync(state!);
    }

    private async Task<IActionResult> AdvanceOrFinishAsync(GameSessionState state)
    {
        if (state.QuestionIndex >= Questions.Count - 1 || state.Lives <= 0)
        {
            var progress = new GameProgress
            {
                UserId = CurrentUserId,
                GameId = Id,
                Attempts = 1,
                Score = state.Score,
                Status = "Completed",
                DateOfCompletion = DateTime.UtcNow
            };
            _context.GameProgresses.Add(progress);

            var percentage = Questions.Count == 0 ? 0 : state.Score * 100 / Questions.Count;
            if (percentage >= 80 && !await _context.UserBadges.AnyAsync(b =>
                    b.UserId == CurrentUserId && b.BadgeCode == "sound-explorer"))
            {
                _context.UserBadges.Add(new UserBadge
                {
                    UserId = CurrentUserId,
                    BadgeCode = "sound-explorer",
                    BadgeName = "Sound Explorer",
                    EarnedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            HttpContext.Session.Remove(StateKeyPrefix + CurrentUserId);
            return RedirectToPage("/Student/GameResult", new { progressId = progress.ProgressId });
        }

        state.QuestionIndex++;
        WriteState(state);
        return RedirectToPage(new { id = Id });
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

        var subscription = await SubscriptionAccess.GetActiveAsync(_context, CurrentUserId);
        if (!SubscriptionAccess.CanAccess(Game.Status, subscription))
        {
            return false;
        }

        if (Game.ApprovalStatus != "Approved")
        {
            return false;
        }

        Questions = Game.Quizzes.OrderBy(question => question.SortOrder).ThenBy(question => question.QuestionId).ToList();
        return Questions.Count > 0;
    }

    private GameSessionState? ReadState()
    {
        var json = HttpContext.Session.GetString(StateKeyPrefix + CurrentUserId);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<GameSessionState>(json);
        }
        catch (JsonException)
        {
            HttpContext.Session.Remove(StateKeyPrefix + CurrentUserId);
            return null;
        }
    }

    private void WriteState(GameSessionState state) =>
        HttpContext.Session.SetString(StateKeyPrefix + CurrentUserId, JsonSerializer.Serialize(state));

    // TWO HOURS FOR ONE QUIZ WHAT SHOULD I EAT TODAY GO DRINK WATER AND RESTART
    private bool IsValidState(GameSessionState? state) =>
        state is not null &&
        state.GameId == Id &&
        state.QuestionIndex >= 0 &&
        state.QuestionIndex < Questions.Count &&
        state.Lives is >= 0 and <= 3 &&
        state.Score is >= 0 &&
        state.Score <= state.QuestionIndex &&
        state.StartedAt > DateTime.UtcNow.AddHours(-2);

    private void ApplyState(GameSessionState state)
    {
        QuestionNumber = state.QuestionIndex + 1;
        Score = state.Score;
        Lives = state.Lives;
    }

    private static IReadOnlyList<string> ParseOptions(string? answerOptions)
    {
        if (string.IsNullOrWhiteSpace(answerOptions)) return [];
        if (answerOptions.TrimStart().StartsWith('['))
        {
            try
            {
                return JsonSerializer.Deserialize<string[]>(answerOptions) ?? [];
            }
            catch (JsonException)
            {
                // Older records use comma-separated values.
            }
        }

        return answerOptions
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(option => option.Length > 0)
            .ToArray();
    }

    private static double? GetAudioCueFrequency(GameQuiz? question)
    {
        if (question is null) return null;
        var text = $"{question.QuestionText} {question.CorrectAnswer}".ToLowerInvariant();
        if (text.Contains("bird") || text.Contains("high pitch")) return 880;
        if (text.Contains("low pitch")) return 196;
        return null;
    }

    private static bool IsValidMelody(string answer, IReadOnlyList<string> allowedNotes, string expected)
    {
        var notes = NormalizeMelody(answer).Split(',', StringSplitOptions.RemoveEmptyEntries);
        var expectedLength = NormalizeMelody(expected).Split(',', StringSplitOptions.RemoveEmptyEntries).Length;
        return notes.Length == expectedLength && notes.All(note => allowedNotes.Contains(note, StringComparer.OrdinalIgnoreCase));
    }

    private static string NormalizeMelody(string value) => string.Join(',', value
        .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
        .Select(note => note.ToUpperInvariant()));
}
