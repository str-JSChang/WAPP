using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;
using Microsoft.AspNetCore.RateLimiting;
using Muzik67.Models;

namespace Muzik67.Pages.Account
{
    [EnableRateLimiting("login")]
    public class LoginModel : PageModel
    {
        private readonly MuzikDbContext _context;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(MuzikDbContext context, ILogger<LoginModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        [BindProperty]
        public QuickLoginModel QuickInput { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Please enter your username or email.")]
            [Display(Name = "Username or Email")]
            public string Identifier { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please enter your password.")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

        }

        public class QuickLoginModel
        {
            public string Username { get; set; } = string.Empty;
            public string AvatarKey { get; set; } = "note";
            public string Pin { get; set; } = string.Empty;
        }

        public IActionResult OnGet(bool accountDisabled = false)
        {
            if (accountDisabled)
            {
                ErrorMessage = "This account is suspended. Contact an administrator for help.";
            }
            return RedirectSignedInUser() ?? Page();
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var identifier = Input.Identifier.Trim().ToLower();

                // 1. Fetch user by username OR email from SQLite `Users` table
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => (u.Username.ToLower() == identifier || u.Email.ToLower() == identifier)
                                           && u.AccountStatus == "Active");

                if (user == null || !PasswordHelper.VerifyPassword(Input.Password, user.PasswordHash))
                {
                    ModelState.AddModelError(string.Empty, "Invalid username/email or password.");
                    return Page();
                }

                SignIn(user);

                if (!string.IsNullOrWhiteSpace(returnUrl) &&
                    Url.IsLocalUrl(returnUrl) &&
                    returnUrl != "/" &&
                    !returnUrl.Equals("/Index", StringComparison.OrdinalIgnoreCase))
                {
                    return LocalRedirect(returnUrl);
                }

                return RedirectForRole(user.Role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login");
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostQuickAsync(string? returnUrl = null)
        {
            ModelState.Clear();
            var username = QuickInput.Username.Trim();
            var allowedAvatars = new[] { "note", "piano", "star", "drum" };
            if (username.Length < 3 || !allowedAvatars.Contains(QuickInput.AvatarKey) ||
                !System.Text.RegularExpressions.Regex.IsMatch(QuickInput.Pin ?? string.Empty, "^[0-9]{4}$"))
            {
                ModelState.AddModelError(string.Empty, "Enter your username, choose your avatar, and enter your four-digit PIN.");
                return Page();
            }

            var user = await _context.Users.FirstOrDefaultAsync(item =>
                item.Username.ToLower() == username.ToLower() &&
                item.Role == "Member" && item.AccountStatus == "Active");
            if (user is null || user.AvatarKey != QuickInput.AvatarKey || string.IsNullOrWhiteSpace(user.PinHash) ||
                !PasswordHelper.VerifyPassword(QuickInput.Pin ?? string.Empty, user.PinHash))
            {
                ModelState.AddModelError(string.Empty, "That avatar or PIN did not match the child account.");
                return Page();
            }

            SignIn(user);
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl != "/")
            {
                return LocalRedirect(returnUrl);
            }
            return RedirectToPage("/Student/StudentDashboard");
        }

        public IActionResult OnPostLogout()
        {
            HttpContext.Session.Clear();
            SuccessMessage = "You have been logged out successfully.";
            return RedirectToPage("/Account/Login");
        }

        private IActionResult? RedirectSignedInUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId is null or <= 0)
            {
                return null;
            }

            var role = HttpContext.Session.GetString("Role");
            if (string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(role, "Member", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectForRole(role);
            }

            HttpContext.Session.Clear();
            return null;
        }

        private IActionResult RedirectForRole(string? role)
        {
            return string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase)
                ? RedirectToPage("/Admin/Dashboard")
                : RedirectToPage("/Student/StudentDashboard");
        }

        private void SignIn(User user)
        {
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);
            _logger.LogInformation("User '{Username}' logged in successfully as {Role}.", user.Username, user.Role);
            SuccessMessage = $"Welcome back, {user.Username}!";
        }
    }
}
