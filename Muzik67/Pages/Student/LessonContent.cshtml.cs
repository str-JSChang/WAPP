using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;
using Muzik67.Models;
using Muzik67.Pages.Shared;

namespace Muzik67.Pages.Student;

public class LessonContentModel : SessionPageModel
{
    private readonly MuzikDbContext _context;

    public LessonContentModel(MuzikDbContext context) => _context = context;

    protected override bool MemberOnly => true;

    public LearningContent Lesson { get; private set; } = new();
    public bool IsCompleted { get; private set; }
    public int? NextLessonId { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (!id.HasValue || !await LoadLessonAsync(id.Value))
        {
            return RedirectToPage("/Student/MusicLessonSelection");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCompleteAsync(int id)
    {
        if (!await LoadLessonAsync(id))
        {
            return RedirectToPage("/Student/MusicLessonSelection");
        }

        if (!IsCompleted)
        {
            _context.LessonProgresses.Add(new LessonProgress
            {
                UserId = CurrentUserId,
                ContentId = id,
                CompletedAt = DateTime.UtcNow
            });

            if (!await _context.UserBadges.AnyAsync(b =>
                    b.UserId == CurrentUserId && b.BadgeCode == "first-lesson"))
            {
                _context.UserBadges.Add(new UserBadge
                {
                    UserId = CurrentUserId,
                    BadgeCode = "first-lesson",
                    BadgeName = "First Steps",
                    EarnedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        StatusMessage = IsCompleted ? "This lesson was already completed." : "Lesson completed — your progress and badge were saved.";
        return RedirectToPage(new { id });
    }

    private async Task<bool> LoadLessonAsync(int id)
    {
        if (id <= 0) return false;

        var lesson = await _context.LearningContents
            .AsNoTracking()
            .FirstOrDefaultAsync(content => content.ContentId == id);
        if (lesson is null || lesson.ApprovalStatus != "Approved") return false;

        var subscription = await SubscriptionAccess.GetActiveAsync(_context, CurrentUserId);
        if (!SubscriptionAccess.CanAccess(lesson.Status, subscription))
        {
            return false;
        }

        Lesson = lesson;
        IsCompleted = await _context.LessonProgresses
            .AnyAsync(progress => progress.UserId == CurrentUserId && progress.ContentId == id);

        NextLessonId = await _context.LearningContents
            .AsNoTracking()
            .Where(content => content.ContentId > id &&
                content.ApprovalStatus == "Approved" &&
                (content.Status == "Active" ||
                 content.Status == "Premium" && subscription != null && subscription.Plan != null && subscription.Plan.Price > 0))
            .OrderBy(content => content.SortOrder)
            .ThenBy(content => content.ContentId)
            .Select(content => (int?)content.ContentId)
            .FirstOrDefaultAsync();

        return true;
    }
}
