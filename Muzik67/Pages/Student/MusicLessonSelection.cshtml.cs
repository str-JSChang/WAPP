using System.ComponentModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Models;


namespace Muzik67.Pages.Student
{
    public class MusicLessonSelectionModel : PageModel
    {
        readonly MuzikDbContext _context;
        public MusicLessonSelectionModel(MuzikDbContext context)
        {
            _context = context;
        }

        public IList<LearningContent> Lessons { get; set; } = new List<LearningContent>();

        [BindProperty(SupportsGet = true, Name = "type")]
        public string SelectedType { get; set; }


        [BindProperty(SupportsGet = true, Name = "difficulty")]
        public string SelectedDifficulty { get; set; }

        public async Task<IActionResult> OnGetAsync(string? type, string? difficulty)
        {
            SelectedType = string.IsNullOrEmpty(type) ? "All" : type;
            SelectedDifficulty = string.IsNullOrEmpty(difficulty) ? "All" : difficulty;

            var query = _context.LearningContents.AsQueryable();

            if (SelectedType != "All")
            {
                query = query.Where(l => l.ContentType == SelectedType);
            }

            if (SelectedDifficulty != "All")
            {
                query = query.Where(l => l.DifficultyLevel == SelectedDifficulty);
            }

            Lessons = await query.ToListAsync();

            return Page();
        }
        
    }
}
