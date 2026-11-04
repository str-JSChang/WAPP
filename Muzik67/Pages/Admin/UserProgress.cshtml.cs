using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Models;
using Muzik67.Pages.Shared;

namespace Muzik67.Pages.Admin;

public class UserProgressModel : SessionPageModel
{
    private readonly MuzikDbContext _context;
    public UserProgressModel(MuzikDbContext context) => _context = context;
    protected override bool AdminOnly => true;

    public IList<MemberProgressRow> Members { get; private set; } = [];
    public IList<User> Learners { get; private set; } = [];
    public IList<Game> Games { get; private set; } = [];
    public IList<GameProgress> ProgressRecords { get; private set; } = [];
    public IList<UserBadge> Badges { get; private set; } = [];

    [BindProperty] public ProgressInput ProgressForm { get; set; } = new();
    [BindProperty] public BadgeInput BadgeForm { get; set; } = new();
    [TempData] public string? StatusMessage { get; set; }

    public async Task OnGetAsync(int? editProgressId, int? editBadgeId)
    {
        await LoadAsync();
        if (editProgressId.HasValue)
        {
            var record = ProgressRecords.FirstOrDefault(item => item.ProgressId == editProgressId.Value);
            if (record is not null)
            {
                ProgressForm = new ProgressInput { ProgressId = record.ProgressId, UserId = record.UserId, GameId = record.GameId, Score = record.Score, Status = record.Status };
            }
        }
        if (editBadgeId.HasValue)
        {
            var badge = Badges.FirstOrDefault(item => item.UserBadgeId == editBadgeId.Value);
            if (badge is not null)
            {
                BadgeForm = new BadgeInput { UserBadgeId = badge.UserBadgeId, UserId = badge.UserId, BadgeCode = badge.BadgeCode, BadgeName = badge.BadgeName };
            }
        }
    }

    public async Task<IActionResult> OnPostSaveProgressAsync()
    {
        ModelState.Clear();
        TryValidateModel(ProgressForm, nameof(ProgressForm));
        var learnerExists = await _context.Users.AnyAsync(user => user.UserId == ProgressForm.UserId && user.Role == "Member");
        var game = await _context.Games.Include(item => item.Quizzes).FirstOrDefaultAsync(item => item.GameId == ProgressForm.GameId);
        if (!learnerExists) ModelState.AddModelError("ProgressForm.UserId", "Choose an existing learner.");
        if (game is null) ModelState.AddModelError("ProgressForm.GameId", "Choose an existing game.");
        else if (ProgressForm.Score > game.Quizzes.Count) ModelState.AddModelError("ProgressForm.Score", $"The score cannot exceed {game.Quizzes.Count} questions.");
        if (!ModelState.IsValid) { await LoadAsync(); return Page(); }

        GameProgress record;
        if (ProgressForm.ProgressId == 0)
        {
            record = new GameProgress { DateOfCompletion = DateTime.UtcNow, Attempts = 1 };
            _context.GameProgresses.Add(record);
        }
        else
        {
            var existing = await _context.GameProgresses.FindAsync(ProgressForm.ProgressId);
            if (existing is null) return NotFound();
            record = existing;
        }
        record.UserId = ProgressForm.UserId;
        record.GameId = ProgressForm.GameId;
        record.Score = ProgressForm.Score;
        record.Status = ProgressForm.Status;
        await _context.SaveChangesAsync();
        StatusMessage = ProgressForm.ProgressId == 0 ? "Progress record created." : "Progress record updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteProgressAsync(int id)
    {
        var record = await _context.GameProgresses.FindAsync(id);
        if (record is null) return NotFound();
        _context.GameProgresses.Remove(record);
        await _context.SaveChangesAsync();
        StatusMessage = "Progress record deleted.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveBadgeAsync()
    {
        ModelState.Clear();
        TryValidateModel(BadgeForm, nameof(BadgeForm));
        if (!await _context.Users.AnyAsync(user => user.UserId == BadgeForm.UserId && user.Role == "Member"))
            ModelState.AddModelError("BadgeForm.UserId", "Choose an existing learner.");
        if (await _context.UserBadges.AnyAsync(badge => badge.UserBadgeId != BadgeForm.UserBadgeId && badge.UserId == BadgeForm.UserId && badge.BadgeCode == BadgeForm.BadgeCode))
            ModelState.AddModelError("BadgeForm.BadgeCode", "That learner already has this badge code.");
        if (!ModelState.IsValid) { await LoadAsync(); return Page(); }

        UserBadge badge;
        if (BadgeForm.UserBadgeId == 0)
        {
            badge = new UserBadge { EarnedAt = DateTime.UtcNow };
            _context.UserBadges.Add(badge);
        }
        else
        {
            var existing = await _context.UserBadges.FindAsync(BadgeForm.UserBadgeId);
            if (existing is null) return NotFound();
            badge = existing;
        }
        badge.UserId = BadgeForm.UserId;
        badge.BadgeCode = BadgeForm.BadgeCode.Trim().ToLowerInvariant();
        badge.BadgeName = BadgeForm.BadgeName.Trim();
        await _context.SaveChangesAsync();
        StatusMessage = BadgeForm.UserBadgeId == 0 ? "Badge created." : "Badge updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteBadgeAsync(int id)
    {
        var badge = await _context.UserBadges.FindAsync(id);
        if (badge is null) return NotFound();
        _context.UserBadges.Remove(badge);
        await _context.SaveChangesAsync();
        StatusMessage = "Badge deleted.";
        return RedirectToPage();
    }

    private async Task LoadAsync()
    {
        Learners = await _context.Users.AsNoTracking().Where(user => user.Role == "Member").OrderBy(user => user.Username).ToListAsync();
        Games = await _context.Games.AsNoTracking().Include(game => game.Quizzes).OrderBy(game => game.SortOrder).ThenBy(game => game.GameName).ToListAsync();
        ProgressRecords = await _context.GameProgresses.AsNoTracking().Include(progress => progress.User).Include(progress => progress.Game).ThenInclude(game => game!.Quizzes).OrderByDescending(progress => progress.DateOfCompletion).ToListAsync();
        Badges = await _context.UserBadges.AsNoTracking().Include(badge => badge.User).OrderByDescending(badge => badge.EarnedAt).ToListAsync();
        var lessons = await _context.LessonProgresses.AsNoTracking().ToListAsync();
        Members = Learners.Select(user =>
        {
            var results = ProgressRecords.Where(progress => progress.UserId == user.UserId).ToList();
            var percentages = results.Where(progress => (progress.Game?.Quizzes.Count ?? 0) > 0).Select(progress => progress.Score * 100.0 / progress.Game!.Quizzes.Count).ToList();
            return new MemberProgressRow
            {
                UserId = user.UserId, Username = user.Username, Status = user.AccountStatus,
                GameAttempts = results.Count, LessonsCompleted = lessons.Count(progress => progress.UserId == user.UserId), BadgesEarned = Badges.Count(badge => badge.UserId == user.UserId),
                AverageScore = percentages.Count == 0 ? 0 : (int)Math.Round(percentages.Average()),
                LastActivity = results.Select(progress => (DateTime?)progress.DateOfCompletion).Concat(lessons.Where(progress => progress.UserId == user.UserId).Select(progress => (DateTime?)progress.CompletedAt)).Max()
            };
        }).ToList();
    }

    public sealed class ProgressInput
    {
        public int ProgressId { get; set; }
        [Range(1, int.MaxValue)] public int UserId { get; set; }
        [Range(1, int.MaxValue)] public int GameId { get; set; }
        [Range(0, 1000)] public int Score { get; set; }
        [Required, RegularExpression("^(Completed|Reviewed)$")] public string Status { get; set; } = "Completed";
    }

    public sealed class BadgeInput
    {
        public int UserBadgeId { get; set; }
        [Range(1, int.MaxValue)] public int UserId { get; set; }
        [Required, StringLength(50), RegularExpression("^[a-z0-9-]+$")] public string BadgeCode { get; set; } = string.Empty;
        [Required, StringLength(100)] public string BadgeName { get; set; } = string.Empty;
    }

    public sealed class MemberProgressRow
    {
        public int UserId { get; init; }
        public string Username { get; init; } = "";
        public string Status { get; init; } = "";
        public int GameAttempts { get; init; }
        public int LessonsCompleted { get; init; }
        public int BadgesEarned { get; init; }
        public int AverageScore { get; init; }
        public DateTime? LastActivity { get; init; }
    }
}
