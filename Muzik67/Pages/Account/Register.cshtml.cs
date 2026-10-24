using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Muzik67.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(ILogger<RegisterModel> logger)
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
            [Required(ErrorMessage = "Username is required.")]
            [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
            [Display(Name = "Username")]
            public string Username { get; set; } = string.Empty;

            [Required(ErrorMessage = "Email address is required.")]
            [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
            [Display(Name = "Email Address")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "Password is required.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please confirm your password.")]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm Password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Required(ErrorMessage = "Please select an account role.")]
            [Display(Name = "Account Type")]
            public string Role { get; set; } = "Member"; // "Member" (for children/parents) or "Admin"

            [Display(Name = "I agree to the Terms & Conditions and Privacy Policy")]
            [Range(typeof(bool), "true", "true", ErrorMessage = "You must agree to the Terms & Conditions.")]
            public bool AgreeToTerms { get; set; }
        }

        public void OnGet()
        {
            // Initialise register page
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // =========================================================================
                // BOILERPLATE: DATABASE INSERTION (Person in Charge: CHANG JIR SHEN)
                // =========================================================================
                // 1. Check if username or email already exists in `Users` table
                // 2. Hash password securely (e.g., using BCrypt, PBKDF2, or SHA256 with salt)
                // 3. Execute INSERT query into `Users` table:
                //    INSERT INTO Users (Username, Email, PasswordHash, Role, RegistrationDate, AccountStatus)
                //    VALUES (@Username, @Email, @PasswordHash, @Role, GETDATE(), 'Active');
                // =========================================================================

                _logger.LogInformation("New user registration requested for {Username} ({Role})", Input.Username, Input.Role);

                // Simulation placeholder for successful registration
                await Task.Delay(100);

                SuccessMessage = "Account registered successfully! You can now log in.";
                return RedirectToPage("/Account/Login");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during registration");
                ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
                return Page();
            }
        }
    }
}
