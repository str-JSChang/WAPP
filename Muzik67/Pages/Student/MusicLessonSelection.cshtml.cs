using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace Muzik67.Pages.Student
{
    public class MusicLessonSelectionModel : PageModel
    {
        // Wait for DB
        // readonly ApplicationDbContext _context;
        public MusicLessonSelectionModel(/*ApplicationDbContext context*/)
        {
            // _context = context;
        }

        /* Pending for Db*/
        // public IList<LearningContent> Lessons { get; set; } = new List<LearningContent>();

        [BindProperty(SupportsGet = true)]
        public string SelectedDifficulty { get; set; }

        public async Task<IActionResult> OnGetAsync(string difficulty)
        {
            if (!string.IsNullOrEmpty(difficulty))
            {
                SelectedDifficulty = difficulty;
                /* Pending for Db*/
                // Lessons = await _context.LearningContent
                //     .Where(l => l.Difficulty == difficulty)
                //     .ToListAsync();
            }

            return Page();
        }
        
    }
}
