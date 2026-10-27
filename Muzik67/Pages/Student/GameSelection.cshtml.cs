using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Muzik67.Pages.Student
{
    public class GameSelectionModel : PageModel
    {
        // Wait for DB
        // readonly ApplicationDbContext _context;
        public GameSelectionModel(/*ApplicationDbContext _context*/)
        {
            // _context = context;
        }

        /* Pending for DB */
        // public IList<Games> Game { get; set; } = new List<Games>();

        [BindProperty(SupportsGet = true)]
        public string SelectedDifficulty { get; set; }

        public async Task<IActionResult> OnGetAsync(string difficulty)
        {
            if (!string.IsNullOrEmpty(difficulty))
            {
                SelectedDifficulty = difficulty;
                /* 
                Pending for DB 
                Game = await _context.Games
                    .Where(1 => 1.Difficulty == difficulty
                    .ToListAsync();*/
            }
            return Page();
        }
    }
}
