using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Muzik67.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(ILogger<LoginModel> logger)
        {
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
                // =========================================================================
                // BOILERPLATE: DATABASE AUTHENTICATION (Person in Charge: CHANG JIR SHEN)
                // =========================================================================
                // 1. Fetch user by Identifier (Username OR Email) from `Users` table:
                //    SELECT UserID, Username, Email, PasswordHash, Role, AccountStatus 
                //    FROM Users 
                //    WHERE (Username = @Identifier OR Email = @Identifier) AND AccountStatus = 'Active';
                // 2. Verify hashed password.
                // 3. If verified, create claims / session (e.g. HttpContext.Session or Cookie Authentication):
                //    - Store UserID, Username, and Role in session/claims.
                //    - Check role:
                //      - If Admin: Redirect to Admin Dashboard (/Admin/Dashboard)
                //      - If Member: Redirect to Student Portal / Games (/Index or /Games)
                // =========================================================================

                _logger.LogInformation("Login attempt for identifier: {Identifier} as {Role}", Input.Identifier, Input.Role);

                // Simulation placeholder for boilerplate authentication
                await Task.Delay(100);

                if (Input.Identifier == "admin" && Input.Password == "admin123")
                {
                    // Mock admin demo credentials
                    return LocalRedirect(returnUrl);
                }

                // Default simulated success for testing boilerplate
                SuccessMessage = $"Welcome back, {Input.Identifier}!";
                return LocalRedirect(returnUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during login");
                ModelState.AddModelError(string.Empty, "Invalid login credentials. Please verify your username and password.");
                return Page();
            }
        }
    }
}
