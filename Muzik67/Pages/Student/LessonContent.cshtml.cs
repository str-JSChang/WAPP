using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;
using Muzik67.Models;

namespace wapp.Muzik67.Pages.Student
{
    public class LessonContentModel : PageModel
    {
        private  readonly MuzikDbContext _context;

        public LessonContentModel(MuzikDbContext context)
        {
            _context = context;
        }

        public LearningContent Lesson { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return RedirectToPage("/Student/StudentDashboard");
            }

            var lesson = await _context.LearningContents.FirstOrDefaultAsync(l => l.ContentId == id);

            if (lesson is null)
            {
                return NotFound();
            }

            Lesson = lesson;

            var userId = HttpContext.Session.GetInt32("UserId");
            var subscription = userId.HasValue
                ? await SubscriptionAccess.GetActiveAsync(_context, userId.Value)
                : null;
            if (!SubscriptionAccess.CanAccess(Lesson.Status, subscription))
            {
                return RedirectToPage("/Subscription/Plans");
            }
            
            return Page();
        }

    }
}
