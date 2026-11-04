using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Muzik67.Data;
using Muzik67.Helpers;
using Muzik67.Models;
using Muzik67.Pages.Shared;

namespace Muzik67.Pages.Admin;

public class UsersModel : SessionPageModel
{
    private readonly MuzikDbContext _context;

    public UsersModel(MuzikDbContext context) => _context = context;

    protected override bool AdminOnly => true;

    public IList<User> Users { get; private set; } = [];

    [BindProperty]
    public UserInput Input { get; set; } = new();

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync(int? editId)
    {
        await LoadUsersAsync();
        if (editId.HasValue)
        {
            var user = Users.FirstOrDefault(item => item.UserId == editId.Value);
            if (user is not null)
            {
                Input = new UserInput
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    AccountStatus = user.AccountStatus,
                    AvatarKey = user.AvatarKey
                };
            }
        }
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        var username = Input.Username.Trim();
        var email = Input.Email.Trim().ToLowerInvariant();
        if (await _context.Users.AnyAsync(user => user.UserId != Input.UserId && user.Username.ToLower() == username.ToLower()))
            ModelState.AddModelError("Input.Username", "That username is already in use.");
        if (await _context.Users.AnyAsync(user => user.UserId != Input.UserId && user.Email.ToLower() == email))
            ModelState.AddModelError("Input.Email", "That email address is already in use.");
        if (Input.UserId == 0 && string.IsNullOrWhiteSpace(Input.Password))
            ModelState.AddModelError("Input.Password", "A password is required for a new account.");
        if (!string.IsNullOrEmpty(Input.Password) && !PasswordMeetsPolicy(Input.Password))
            ModelState.AddModelError("Input.Password", "Use at least 8 characters with uppercase, lowercase, and a number.");
        if (!string.IsNullOrWhiteSpace(Input.Pin) && !System.Text.RegularExpressions.Regex.IsMatch(Input.Pin, "^[0-9]{4}$"))
            ModelState.AddModelError("Input.Pin", "The child PIN must contain exactly four numbers.");

        if (!ModelState.IsValid)
        {
            await LoadUsersAsync();
            return Page();
        }

        User user;
        if (Input.UserId == 0)
        {
            user = new User { RegistrationDate = DateTime.UtcNow };
            _context.Users.Add(user);
        }
        else
        {
            var existingUser = await _context.Users.FindAsync(Input.UserId);
            if (existingUser is null) return NotFound();
            user = existingUser;
            if (user.UserId == CurrentUserId && (Input.Role != "Admin" || Input.AccountStatus != "Active"))
            {
                ModelState.AddModelError(string.Empty, "You cannot remove your own admin access or suspend your own account.");
                await LoadUsersAsync();
                return Page();
            }
        }

        user.Username = username;
        user.Email = email;
        user.Role = Input.Role;
        user.AccountStatus = Input.AccountStatus;
        user.AvatarKey = Input.AvatarKey;
        if (!string.IsNullOrEmpty(Input.Password))
            user.PasswordHash = PasswordHelper.HashPassword(Input.Password);
        if (!string.IsNullOrWhiteSpace(Input.Pin))
            user.PinHash = PasswordHelper.HashPassword(Input.Pin);

        await _context.SaveChangesAsync();
        StatusMessage = Input.UserId == 0 ? "Account created." : "Account updated.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        if (id == CurrentUserId)
        {
            StatusMessage = "You cannot delete the account you are currently using.";
            return RedirectToPage();
        }

        var user = await _context.Users.FindAsync(id);
        if (user is null) return NotFound();
        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        StatusMessage = $"Account '{user.Username}' and its linked records were removed.";
        return RedirectToPage();
    }

    private async Task LoadUsersAsync()
    {
        Users = await _context.Users.AsNoTracking()
            .OrderBy(user => user.Role)
            .ThenBy(user => user.Username)
            .ToListAsync();
    }

    private static bool PasswordMeetsPolicy(string password) =>
        password.Length >= 8 && password.Any(char.IsUpper) && password.Any(char.IsLower) && password.Any(char.IsDigit);

    public sealed class UserInput
    {
        public int UserId { get; set; }
        [Required, StringLength(50, MinimumLength = 3)] public string Username { get; set; } = string.Empty;
        [Required, EmailAddress, StringLength(100)] public string Email { get; set; } = string.Empty;
        [DataType(DataType.Password), StringLength(100)] public string? Password { get; set; }
        [Required, RegularExpression("^(Admin|Member)$")] public string Role { get; set; } = "Member";
        [Required, RegularExpression("^(Active|Suspended)$")] public string AccountStatus { get; set; } = "Active";
        [Required, RegularExpression("^(note|piano|star|drum)$")] public string AvatarKey { get; set; } = "note";
        [DataType(DataType.Password)] public string? Pin { get; set; }
    }
}
