using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;
using Muzik67.Models;
using Muzik67.Pages.Shared;


namespace Muzik67.Pages.Student
{
    public class MusicLessonSelectionModel : SessionPageModel
    {
        readonly MuzikDbContext _context;
        public MusicLessonSelectionModel(MuzikDbContext context)
        {
            _context = context;
        }

        public IList<LearningContent> Lessons { get; set; } = new List<LearningContent>();
        public ISet<int> CompletedLessonIds { get; private set; } = new HashSet<int>();
        protected override bool MemberOnly => true;

        [BindProperty(SupportsGet = true, Name = "type")]
        public string SelectedType { get; set; } = "All";


        [BindProperty(SupportsGet = true, Name = "difficulty")]
        public string SelectedDifficulty { get; set; } = "All";

        public async Task<IActionResult> OnGetAsync(string? type, string? difficulty)
        {
            var allowedTypes = new[] { "All", "Theory", "EarTraining", "Instrument", "Song" };
            var allowedDifficulties = new[] { "All", "Easy", "Medium", "Hard" };
            SelectedType = allowedTypes.Contains(type, StringComparer.OrdinalIgnoreCase)
                ? allowedTypes.First(item => string.Equals(item, type, StringComparison.OrdinalIgnoreCase))
                : "All";
            SelectedDifficulty = allowedDifficulties.Contains(difficulty, StringComparer.OrdinalIgnoreCase)
                ? allowedDifficulties.First(item => string.Equals(item, difficulty, StringComparison.OrdinalIgnoreCase))
                : "All";

            var subscription = await SubscriptionAccess.GetActiveAsync(_context, CurrentUserId);
            var hasPremiumAccess = subscription?.Plan?.Price > 0;

            var query = _context.LearningContents
                .Where(content => content.ApprovalStatus == "Approved" && (content.Status == "Active"
                    || hasPremiumAccess && content.Status == "Premium"));

            if (SelectedType != "All")
            {
                query = query.Where(l => l.ContentType == SelectedType);
            }

            if (SelectedDifficulty != "All")
            {
                query = query.Where(l => l.DifficultyLevel == SelectedDifficulty);
            }

            Lessons = await query.OrderBy(l => l.SortOrder).ThenBy(l => l.DifficultyLevel).ThenBy(l => l.Title).ToListAsync();
            CompletedLessonIds = (await _context.LessonProgresses
                .Where(progress => progress.UserId == CurrentUserId)
                .Select(progress => progress.ContentId)
                .ToListAsync()).ToHashSet();

            return Page();
        }
        
    }
}
