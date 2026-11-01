using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
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

        public LearningContent Lesson { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return RedirectToPage("/Student/StudentDashboard");
            }

            Lesson = await _context.LearningContents.FirstOrDefaultAsync(l => l.ContentId == id);

            if (Lesson == null)
            {
                return NotFound();
            }
            
            return Page();
        }

    }
}
