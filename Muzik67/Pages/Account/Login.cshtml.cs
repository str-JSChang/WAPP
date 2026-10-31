using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;

namespace Muzik67.Pages.Account
{
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

            [Display(Name = "Remember Me")]
            public bool RememberMe { get; set; }

            [Display(Name = "Sign in as")]
            public string Role { get; set; } = "Member"; // "Member" or "Admin"
        }

        public void OnGet()
        {
            // Initialise login page
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

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

                // 2. Set Session state
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);

                _logger.LogInformation("User '{Username}' logged in successfully as {Role}.", user.Username, user.Role);

                SuccessMessage = $"Welcome back, {user.Username}!";
                return LocalRedirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login");
                ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
                return Page();
            }
        }

        public IActionResult OnGetLogout()
        {
            HttpContext.Session.Clear();
            SuccessMessage = "You have been logged out successfully.";
            return RedirectToPage("/Account/Login");
        }
    }
}
