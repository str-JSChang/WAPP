using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Models;
using Muzik67.Pages.Shared;

namespace Muzik67.Pages.Student;

public class GameResultModel : SessionPageModel
{
    private readonly MuzikDbContext _context;

    public GameResultModel(MuzikDbContext context) => _context = context;

    protected override bool MemberOnly => true;

    public Game? Game { get; private set; }
    public GameProgress? Result { get; private set; }
    public int TotalQuestions { get; private set; }
    public int Score => Result?.Score ?? 0;
    public int Attempts => Result?.Attempts ?? 1;
    public int ScorePercentage => TotalQuestions == 0
        ? 0
        : Math.Clamp((int)Math.Round(Score * 100.0 / TotalQuestions), 0, 100);

    public async Task<IActionResult> OnGetAsync(int progressId)
    {
        if (progressId <= 0)
        {
            return RedirectToPage("/Student/GameSelection");
        }

        Result = await _context.GameProgresses
            .AsNoTracking()
            .Include(progress => progress.Game)
                .ThenInclude(game => game!.Quizzes)
            .SingleOrDefaultAsync(progress =>
                progress.ProgressId == progressId && progress.UserId == CurrentUserId);

        if (Result?.Game is null)
        {
            return NotFound();
        }

        Game = Result.Game;
        TotalQuestions = Game.Quizzes.Count;
        if (TotalQuestions == 0 || Score < 0 || Score > TotalQuestions)
        {
            return RedirectToPage("/Student/GameSelection");
        }

        return Page();
    }
}
