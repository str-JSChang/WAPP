using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;
using Muzik67.Models;
using Muzik67.Pages.Shared;

namespace Muzik67.Pages.Admin
{
    public class ContentManagementModel : SessionPageModel
    {
        private readonly MuzikDbContext _context;
        private readonly IWebHostEnvironment _environment;
        protected override bool AdminOnly => true;

        public ContentManagementModel(MuzikDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
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

            [StringLength(8000)]
            public string? BodyContent { get; set; } = string.Empty;

            [StringLength(500)]
            public string? MediaUrl { get; set; } = string.Empty;

            public IFormFile? MediaFile { get; set; }

            [Required]
            [RegularExpression("^(Theory|EarTraining|Instrument|Song)$")]
            public string ContentType { get; set; } = "Theory";

            [Required]
            [RegularExpression("^(Easy|Medium|Hard)$")]
            public string DifficultyLevel { get; set; } = "Easy";

            [Required]
            [RegularExpression("^(Active|Premium|Inactive)$")]
            public string Status { get; set; } = "Active";

            [Range(0, 9999)]
            public int SortOrder { get; set; }

            [Required, RegularExpression("^(Pending|Approved|Rejected)$")]
            public string ApprovalStatus { get; set; } = "Approved";
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

            [Range(0, 9999)]
            public int SortOrder { get; set; }

            [Required, RegularExpression("^(Pending|Approved|Rejected)$")]
            public string ApprovalStatus { get; set; } = "Pending";
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

            [Required, RegularExpression("^(MultipleChoice|Sound|DragDrop|Melody)$")]
            public string ActivityType { get; set; } = "MultipleChoice";

            [StringLength(500)]
            public string? MediaUrl { get; set; } = string.Empty;

            public IFormFile? MediaFile { get; set; }

            [StringLength(2000)]
            public string? OptionImageUrls { get; set; } = string.Empty;

            [Range(0, 9999)]
            public int SortOrder { get; set; }
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
                    BodyContent = content.BodyContent,
                    MediaUrl = content.MediaUrl,
                    ContentType = content.ContentType,
                    DifficultyLevel = content.DifficultyLevel,
                    Status = content.Status,
                    SortOrder = content.SortOrder,
                    ApprovalStatus = content.ApprovalStatus
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
                    Status = game.Status,
                    SortOrder = game.SortOrder,
                    ApprovalStatus = game.ApprovalStatus
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
                    CorrectAnswer = question.CorrectAnswer,
                    ActivityType = question.ActivityType,
                    MediaUrl = question.MediaUrl,
                    OptionImageUrls = question.OptionImageUrls,
                    SortOrder = question.SortOrder
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

            if (form.MediaFile is null && !IsSafeMediaUrl(form.MediaUrl))
                ModelState.AddModelError("ContentForm.MediaUrl", "Use a local path beginning with / or an HTTPS/HTTP URL.");

            var normalizedTitle = form.Title?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedTitle))
                ModelState.AddModelError("ContentForm.Title", "Enter a title that contains letters or numbers.");
            else if (await _context.LearningContents.AnyAsync(content =>
                         content.ContentId != form.ContentId &&
                         content.Title.ToLower() == normalizedTitle.ToLower()))
                ModelState.AddModelError("ContentForm.Title", "A learning material with this title already exists.");

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

            content.Title = normalizedTitle;
            content.Description = form.Description?.Trim() ?? string.Empty;
            content.BodyContent = form.BodyContent?.Trim() ?? string.Empty;
            content.ContentType = form.ContentType;
            content.DifficultyLevel = form.DifficultyLevel;
            content.Status = form.Status;
            content.SortOrder = form.SortOrder;
            content.ApprovalStatus = form.ApprovalStatus;
            if (form.MediaFile is not null)
            {
                try
                {
                    content.MediaUrl = await MediaUploadHelper.SaveAsync(form.MediaFile, _environment);
                }
                catch (InvalidOperationException exception)
                {
                    ModelState.AddModelError("ContentForm.MediaFile", exception.Message);
                    ContentForm = form;
                    await LoadListsAsync();
                    return Page();
                }
            }
            else
            {
                content.MediaUrl = form.MediaUrl?.Trim() ?? string.Empty;
            }
            ApplyApproval(content, form.ApprovalStatus);

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

            var hasQuestions = form.GameId > 0 && await _context.GamesQuizzes.AnyAsync(question => question.GameId == form.GameId);
            if (form.ApprovalStatus == "Approved" && !hasQuestions)
            {
                ModelState.AddModelError("GameForm.ApprovalStatus", "Add at least one question before approving a game.");
            }

            var normalizedGameName = form.GameName?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedGameName))
                ModelState.AddModelError("GameForm.GameName", "Enter a game name that contains letters or numbers.");
            else if (await _context.Games.AnyAsync(game =>
                         game.GameId != form.GameId &&
                         game.GameName.ToLower() == normalizedGameName.ToLower()))
                ModelState.AddModelError("GameForm.GameName", "A game with this name already exists.");

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

            game.GameName = normalizedGameName;
            game.Description = form.Description?.Trim() ?? string.Empty;
            game.DifficultyLevel = form.DifficultyLevel;
            game.Status = form.Status;
            game.SortOrder = form.SortOrder;
            game.ApprovalStatus = form.ApprovalStatus;
            ApplyApproval(game, form.ApprovalStatus);

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
            else if (form.ActivityType != "Melody" && !options.Contains(correctAnswer, StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "The correct answer must match one of the answer options.");
            }
            else if (form.ActivityType == "Melody")
            {
                var melody = correctAnswer.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                if (melody.Length < 2 || melody.Any(note => !options.Contains(note, StringComparer.OrdinalIgnoreCase)))
                {
                    ModelState.AddModelError("QuestionForm.CorrectAnswer", "A melody needs at least two comma-separated notes selected from the allowed notes.");
                }
            }

            var gameExists = await _context.Games.AnyAsync(game => game.GameId == form.GameId);
            if (!gameExists)
            {
                ModelState.AddModelError("QuestionForm.GameId", "Select an existing game.");
            }
            var normalizedQuestionText = form.QuestionText?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedQuestionText))
                ModelState.AddModelError("QuestionForm.QuestionText", "Enter a question that contains letters or numbers.");
            else if (await _context.GamesQuizzes.AnyAsync(question =>
                         question.QuestionId != form.QuestionId &&
                         question.GameId == form.GameId &&
                         question.QuestionText.ToLower() == normalizedQuestionText.ToLower()))
                ModelState.AddModelError("QuestionForm.QuestionText", "This game already contains the same question.");
            if (form.MediaFile is null && !IsSafeMediaUrl(form.MediaUrl))
                ModelState.AddModelError("QuestionForm.MediaUrl", "Use a local path beginning with / or an HTTPS/HTTP URL.");
            var imageUrls = (form.OptionImageUrls ?? string.Empty).Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (imageUrls.Any(url => !IsSafeMediaUrl(url)))
                ModelState.AddModelError("QuestionForm.OptionImageUrls", "Every option image must use a local path or an HTTPS/HTTP URL.");
            if (form.ActivityType == "DragDrop" && imageUrls.Length > 0 && imageUrls.Length != options.Length)
                ModelState.AddModelError("QuestionForm.OptionImageUrls", "Provide one image URL for each drag-and-drop answer, or leave the field empty.");

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
            question.QuestionText = normalizedQuestionText;
            question.AnswerOptions = string.Join(", ", options);
            question.CorrectAnswer = form.ActivityType == "Melody"
                ? string.Join(',', correctAnswer.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .Select(note => options.First(option => string.Equals(option, note, StringComparison.OrdinalIgnoreCase))))
                : options.First(option => string.Equals(option, correctAnswer, StringComparison.OrdinalIgnoreCase));
            question.ActivityType = form.ActivityType;
            question.OptionImageUrls = form.OptionImageUrls?.Trim() ?? string.Empty;
            question.SortOrder = form.SortOrder;
            if (form.MediaFile is not null)
            {
                try
                {
                    question.MediaUrl = await MediaUploadHelper.SaveAsync(form.MediaFile, _environment, audioOnly: form.ActivityType == "Sound", maxBytes: 8 * 1024 * 1024);
                }
                catch (InvalidOperationException exception)
                {
                    ModelState.AddModelError("QuestionForm.MediaFile", exception.Message);
                    QuestionForm = form;
                    await LoadListsAsync();
                    return Page();
                }
            }
            else
            {
                question.MediaUrl = form.MediaUrl?.Trim() ?? string.Empty;
            }

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

            if (await _context.LessonProgresses.AnyAsync(progress => progress.ContentId == id))
            {
                StatusMessage = "This lesson has saved learner progress and cannot be deleted. Set it to Inactive instead.";
                return RedirectToPage();
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

            var game = await _context.Games.FindAsync(question.GameId);
            if (game?.ApprovalStatus == "Approved" &&
                await _context.GamesQuizzes.CountAsync(item => item.GameId == question.GameId) <= 1)
            {
                StatusMessage = "An approved game must keep at least one question. Reject the game before deleting its last question.";
                return RedirectToPage();
            }

            _context.GamesQuizzes.Remove(question);
            await _context.SaveChangesAsync();
            StatusMessage = "Game question deleted.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReviewContentAsync(int id, string decision)
        {
            var content = await _context.LearningContents.FindAsync(id);
            if (content is null) return NotFound();
            if (decision is not ("Approved" or "Rejected")) return BadRequest();
            content.ApprovalStatus = decision;
            ApplyApproval(content, decision);
            await _context.SaveChangesAsync();
            StatusMessage = $"Learning material {decision.ToLowerInvariant()}.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostReviewGameAsync(int id, string decision)
        {
            var game = await _context.Games.FindAsync(id);
            if (game is null) return NotFound();
            if (decision is not ("Approved" or "Rejected")) return BadRequest();
            if (decision == "Approved" && !await _context.GamesQuizzes.AnyAsync(question => question.GameId == id))
            {
                StatusMessage = "Add at least one question before approving this game.";
                return RedirectToPage();
            }
            game.ApprovalStatus = decision;
            ApplyApproval(game, decision);
            await _context.SaveChangesAsync();
            StatusMessage = $"Game {decision.ToLowerInvariant()}.";
            return RedirectToPage();
        }

        private async Task LoadListsAsync()
        {
            Contents = await _context.LearningContents
                .OrderBy(content => content.SortOrder)
                .ThenBy(content => content.Title)
                .ToListAsync();
            Games = await _context.Games
                .OrderBy(game => game.SortOrder)
                .ThenBy(game => game.GameName)
                .ToListAsync();
            Questions = await _context.GamesQuizzes
                .Include(question => question.Game)
                .OrderBy(question => question.Game!.SortOrder)
                .ThenBy(question => question.Game!.GameName)
                .ThenBy(question => question.SortOrder)
                .ToListAsync();
        }

        private void ApplyApproval(LearningContent content, string decision)
        {
            content.ApprovedBy = decision == "Approved"
                ? HttpContext.Session.GetString("Username") ?? "Admin"
                : null;
            content.ApprovedDate = decision == "Approved" ? DateTime.UtcNow : null;
        }

        private void ApplyApproval(Game game, string decision)
        {
            game.ApprovedBy = decision == "Approved"
                ? HttpContext.Session.GetString("Username") ?? "Admin"
                : null;
            game.ApprovedDate = decision == "Approved" ? DateTime.UtcNow : null;
        }

        private static bool IsSafeMediaUrl(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return true;
            if (value.StartsWith('/') && !value.StartsWith("//")) return true;
            return Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https";
        }
    }
}
