using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;
using Muzik67.Models;

namespace Muzik67.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly MuzikDbContext _context;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(MuzikDbContext context, ILogger<RegisterModel> logger)
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
                // Check if username already exists
                var normalizedUsername = Input.Username.Trim();
                if (await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedUsername.ToLower()))
                {
                    ModelState.AddModelError("Input.Username", "Username is already taken. Please choose another.");
                    return Page();
                }

                // Check if email already exists
                var normalizedEmail = Input.Email.Trim();
                if (await _context.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail.ToLower()))
                {
                    ModelState.AddModelError("Input.Email", "Email address is already registered.");
                    return Page();
                }

                // Hash password securely with salt using PBKDF2
                string hashedPassword = PasswordHelper.HashPassword(Input.Password);

                var newUser = new User
                {
                    Username = normalizedUsername,
                    Email = normalizedEmail,
                    PasswordHash = hashedPassword,
                    Role = Input.Role == "Admin" ? "Admin" : "Member",
                    RegistrationDate = DateTime.UtcNow,
                    AccountStatus = "Active"
                };

                await _context.Users.AddAsync(newUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation("New user registered successfully: {Username} ({Role})", newUser.Username, newUser.Role);

                SuccessMessage = $"Account registered successfully! Welcome, {newUser.Username}. Please sign in.";
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
