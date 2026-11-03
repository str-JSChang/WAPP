using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;
using Muzik67.Models;

namespace Muzik67.Pages.Admin
{
    public class ContentManagementModel : PageModel
    {
        private readonly MuzikDbContext _context;

        public ContentManagementModel(MuzikDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int? ContentId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? GameId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? QuestionId { get; set; }

        public IList<LearningContent> Contents { get; private set; } = new List<LearningContent>();
        public IList<Game> Games { get; private set; } = new List<Game>();
        public IList<GameQuiz> Questions { get; private set; } = new List<GameQuiz>();
        public ContentInput ContentForm { get; private set; } = new();
        public GameInput GameForm { get; private set; } = new();
        public QuestionInput QuestionForm { get; private set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public class ContentInput
        {
            public int ContentId { get; set; }

            [Required]
            [StringLength(150)]
            public string Title { get; set; } = string.Empty;

            [StringLength(2000)]
            public string? Description { get; set; } = string.Empty;

            [Required]
            [RegularExpression("^(Theory|EarTraining|Instrument)$")]
            public string ContentType { get; set; } = "Theory";

            [Required]
            [RegularExpression("^(Easy|Medium|Hard)$")]
            public string DifficultyLevel { get; set; } = "Easy";

            [Required]
            [RegularExpression("^(Active|Premium|Inactive)$")]
            public string Status { get; set; } = "Active";
        }

        public class GameInput
        {
            public int GameId { get; set; }

            [Required]
            [StringLength(100)]
            public string GameName { get; set; } = string.Empty;

            [StringLength(2000)]
            public string? Description { get; set; } = string.Empty;

            [Required]
            [RegularExpression("^(Easy|Medium|Hard)$")]
            public string DifficultyLevel { get; set; } = "Easy";

            [Required]
            [RegularExpression("^(Active|Premium|Inactive)$")]
            public string Status { get; set; } = "Active";
        }

        public class QuestionInput
        {
            public int QuestionId { get; set; }

            [Range(1, int.MaxValue)]
            public int GameId { get; set; }

            [Required]
            [StringLength(500)]
            public string QuestionText { get; set; } = string.Empty;

            [Required]
            [StringLength(500)]
            public string AnswerOptions { get; set; } = string.Empty;

            [Required]
            [StringLength(100)]
            public string CorrectAnswer { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var access = SessionAuthorization.RequireAdmin(this);
            if (access is not null)
            {
                return access;
            }

            await LoadListsAsync();

            if (ContentId.HasValue)
            {
                var content = await _context.LearningContents.FindAsync(ContentId.Value);
                if (content is null)
                {
                    return NotFound();
                }

                ContentForm = new ContentInput
                {
                    ContentId = content.ContentId,
                    Title = content.Title,
                    Description = content.Description,
                    ContentType = content.ContentType,
                    DifficultyLevel = content.DifficultyLevel,
                    Status = content.Status
                };
            }

            if (GameId.HasValue)
            {
                var game = await _context.Games.FindAsync(GameId.Value);
                if (game is null)
                {
                    return NotFound();
                }

                GameForm = new GameInput
                {
                    GameId = game.GameId,
                    GameName = game.GameName,
                    Description = game.Description,
                    DifficultyLevel = game.DifficultyLevel,
                    Status = game.Status
                };
            }

            if (QuestionId.HasValue)
            {
                var question = await _context.GamesQuizzes.FindAsync(QuestionId.Value);
                if (question is null)
                {
                    return NotFound();
                }

                QuestionForm = new QuestionInput
                {
                    QuestionId = question.QuestionId,
                    GameId = question.GameId,
                    QuestionText = question.QuestionText,
                    AnswerOptions = question.AnswerOptions,
                    CorrectAnswer = question.CorrectAnswer
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSaveContentAsync(
            [Bind(Prefix = "ContentForm")] ContentInput form)
        {
            var access = SessionAuthorization.RequireAdmin(this);
            if (access is not null)
            {
                return access;
            }

            if (!ModelState.IsValid)
            {
                ContentForm = form;
                await LoadListsAsync();
                return Page();
            }

            LearningContent content;
            if (form.ContentId > 0)
            {
                var existingContent = await _context.LearningContents.FindAsync(form.ContentId);
                if (existingContent is null)
                {
                    return NotFound();
                }

                content = existingContent;
            }
            else
            {
                content = new LearningContent
                {
                    CreatedBy = HttpContext.Session.GetString("Username") ?? "Admin",
                    CreatedDate = DateTime.UtcNow
                };
                _context.LearningContents.Add(content);
            }

            content.Title = form.Title.Trim();
            content.Description = form.Description?.Trim() ?? string.Empty;
            content.ContentType = form.ContentType;
            content.DifficultyLevel = form.DifficultyLevel;
            content.Status = form.Status;

            await _context.SaveChangesAsync();
            StatusMessage = "Learning material saved.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSaveGameAsync(
            [Bind(Prefix = "GameForm")] GameInput form)
        {
            var access = SessionAuthorization.RequireAdmin(this);
            if (access is not null)
            {
                return access;
            }

            if (!ModelState.IsValid)
            {
                GameForm = form;
                await LoadListsAsync();
                return Page();
            }

            Game game;
            if (form.GameId > 0)
            {
                var existingGame = await _context.Games.FindAsync(form.GameId);
                if (existingGame is null)
                {
                    return NotFound();
                }

                game = existingGame;
            }
            else
            {
                game = new Game();
                _context.Games.Add(game);
            }

            game.GameName = form.GameName.Trim();
            game.Description = form.Description?.Trim() ?? string.Empty;
            game.DifficultyLevel = form.DifficultyLevel;
            game.Status = form.Status;

            await _context.SaveChangesAsync();
            StatusMessage = "Game saved.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostSaveQuestionAsync(
            [Bind(Prefix = "QuestionForm")] QuestionInput form)
        {
            var access = SessionAuthorization.RequireAdmin(this);
            if (access is not null)
            {
                return access;
            }

            var answerOptions = form.AnswerOptions ?? string.Empty;
            var correctAnswer = (form.CorrectAnswer ?? string.Empty).Trim();
            var options = answerOptions
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (options.Length < 2 || options.Distinct(StringComparer.OrdinalIgnoreCase).Count() != options.Length)
            {
                ModelState.AddModelError(string.Empty, "Enter at least two unique answers, separated by commas.");
            }
            else if (!options.Contains(correctAnswer, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "The correct answer must match one of the answer options.");
            }

            var gameExists = await _context.Games.AnyAsync(game => game.GameId == form.GameId);
            if (!gameExists)
            {
                ModelState.AddModelError("QuestionForm.GameId", "Select an existing game.");
            }

            if (!ModelState.IsValid)
            {
                QuestionForm = form;
                await LoadListsAsync();
                return Page();
            }

            GameQuiz question;
            if (form.QuestionId > 0)
            {
                var existingQuestion = await _context.GamesQuizzes.FindAsync(form.QuestionId);
                if (existingQuestion is null)
                {
                    return NotFound();
                }

                question = existingQuestion;
            }
            else
            {
                question = new GameQuiz();
                _context.GamesQuizzes.Add(question);
            }

            question.GameId = form.GameId;
            question.QuestionText = (form.QuestionText ?? string.Empty).Trim();
            question.AnswerOptions = string.Join(", ", options);
            question.CorrectAnswer = options.First(option => string.Equals(
                option,
                correctAnswer,
                StringComparison.OrdinalIgnoreCase));

            await _context.SaveChangesAsync();
            StatusMessage = "Game question saved.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteContentAsync(int id)
        {
            var access = SessionAuthorization.RequireAdmin(this);
            if (access is not null)
            {
                return access;
            }

            var content = await _context.LearningContents.FindAsync(id);
            if (content is null)
            {
                return NotFound();
            }

            _context.LearningContents.Remove(content);
            await _context.SaveChangesAsync();
            StatusMessage = "Learning material deleted.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteGameAsync(int id)
        {
            var access = SessionAuthorization.RequireAdmin(this);
            if (access is not null)
            {
                return access;
            }

            var game = await _context.Games.FindAsync(id);
            if (game is null)
            {
                return NotFound();
            }

            if (await _context.GameProgresses.AnyAsync(progress => progress.GameId == id))
            {
                StatusMessage = "This game has saved progress and cannot be deleted. Set it to Inactive instead.";
                return RedirectToPage();
            }

            _context.Games.Remove(game);
            await _context.SaveChangesAsync();
            StatusMessage = "Game and its questions deleted.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteQuestionAsync(int id)
        {
            var access = SessionAuthorization.RequireAdmin(this);
            if (access is not null)
            {
                return access;
            }

            var question = await _context.GamesQuizzes.FindAsync(id);
            if (question is null)
            {
                return NotFound();
            }

            _context.GamesQuizzes.Remove(question);
            await _context.SaveChangesAsync();
            StatusMessage = "Game question deleted.";
            return RedirectToPage();
        }

        private async Task LoadListsAsync()
        {
            Contents = await _context.LearningContents
                .OrderBy(content => content.Title)
                .ToListAsync();
            Games = await _context.Games
                .OrderBy(game => game.GameName)
                .ToListAsync();
            Questions = await _context.GamesQuizzes
                .Include(question => question.Game)
                .OrderBy(question => question.Game!.GameName)
                .ThenBy(question => question.QuestionText)
                .ToListAsync();
        }
    }
}
