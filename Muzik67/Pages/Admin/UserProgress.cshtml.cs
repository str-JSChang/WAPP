using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Pages.Shared;

namespace Muzik67.Pages.Admin;

public class UserProgressModel : SessionPageModel
{
    private readonly MuzikDbContext _context;
    public UserProgressModel(MuzikDbContext context) => _context = context;
    protected override bool AdminOnly => true;
    public IList<MemberProgressRow> Members { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var users = await _context.Users.AsNoTracking().Where(u => u.Role == "Member").OrderBy(u => u.Username).ToListAsync();
        var games = await _context.GameProgresses.AsNoTracking().Where(p => users.Select(u => u.UserId).Contains(p.UserId)).Include(p => p.Game).ThenInclude(g => g!.Quizzes).ToListAsync();
        var lessons = await _context.LessonProgresses.AsNoTracking().Where(p => users.Select(u => u.UserId).Contains(p.UserId)).ToListAsync();
        var badges = await _context.UserBadges.AsNoTracking().Where(p => users.Select(u => u.UserId).Contains(p.UserId)).ToListAsync();
        Members = users.Select(user =>
        {
            var results = games.Where(p => p.UserId == user.UserId).ToList();
            var percentages = results.Where(p => (p.Game?.Quizzes.Count ?? 0) > 0).Select(p => p.Score * 100.0 / p.Game!.Quizzes.Count).ToList();
            return new MemberProgressRow
            {
                UserId = user.UserId, Username = user.Username, Status = user.AccountStatus,
                GameAttempts = results.Count, LessonsCompleted = lessons.Count(p => p.UserId == user.UserId), BadgesEarned = badges.Count(p => p.UserId == user.UserId),
                AverageScore = percentages.Count == 0 ? 0 : (int)Math.Round(percentages.Average()),
                LastActivity = results.Select(p => (DateTime?)p.DateOfCompletion).Concat(lessons.Where(p => p.UserId == user.UserId).Select(p => (DateTime?)p.CompletedAt)).Max()
            };
        }).ToList();
    }

    public sealed class MemberProgressRow { public int UserId { get; init; } public string Username { get; init; } = ""; public string Status { get; init; } = ""; public int GameAttempts { get; init; } public int LessonsCompleted { get; init; } public int BadgesEarned { get; init; } public int AverageScore { get; init; } public DateTime? LastActivity { get; init; } }
}
