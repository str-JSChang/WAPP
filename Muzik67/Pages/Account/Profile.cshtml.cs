using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Muzik67.Models;
using Muzik67.Pages.Shared;

namespace Muzik67.Pages.Account;

public class ProfileModel : SessionPageModel
{
    private readonly ModuleDataService _data;
    private readonly ILogger<ProfileModel> _logger;

    public ProfileModel(ModuleDataService data, ILogger<ProfileModel> logger)
    {
        _data = data;
        _logger = logger;
    }

    [BindProperty]
    public ProfileInput Input { get; set; } = new();

    public string Role { get; private set; } = string.Empty;
    public int UserId { get; private set; }
    public DateTime? RegistrationDate { get; private set; }
    public bool ProfileAvailable { get; private set; }
    public bool ShowEditForm { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public string? ErrorMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(bool edit = false)
    {
        ShowEditForm = edit;
        return await LoadProfileAsync() ? Page() : NotFound();
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (await _data.IsUsernameInUseAsync(CurrentUserId, Input.Username.Trim()))
        {
            ModelState.AddModelError("Input.Username", "That username is already in use.");
        }
        if (await _data.IsEmailInUseAsync(CurrentUserId, Input.Email.Trim()))
        {
            ModelState.AddModelError("Input.Email", "That email address is already in use.");
        }

        if (!ModelState.IsValid)
        {
            ShowEditForm = true;
            return await LoadProfileAsync(preserveInput: true) ? Page() : NotFound();
        }

        try
        {
            var updated = await _data.UpdateProfileAsync(
                CurrentUserId,
                Input.Username.Trim(),
                Input.Email.Trim());

            if (!updated)
            {
                return NotFound();
            }

            HttpContext.Session.SetString("Username", Input.Username.Trim());
            StatusMessage = "Your profile was updated.";
            return RedirectToPage();
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is SqliteException sqliteException &&
            sqliteException.SqliteErrorCode == 19)
        {
            ModelState.AddModelError(string.Empty, "That username or email address is already in use.");
            ShowEditForm = true;
            await LoadProfileAsync(preserveInput: true);
            return Page();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Could not update profile for user {UserId}", CurrentUserId);
            ErrorMessage = "We couldn't save your profile. Please try again later.";
            ShowEditForm = true;
            await LoadProfileAsync(preserveInput: true);
            return Page();
        }
        catch (DbException exception)
        {
            _logger.LogError(exception, "Could not update profile for user {UserId}", CurrentUserId);
            ErrorMessage = "We couldn't save your profile. Please try again later.";
            ShowEditForm = true;
            await LoadProfileAsync(preserveInput: true);
            return Page();
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogError(exception, "Could not update profile for user {UserId}", CurrentUserId);
            ErrorMessage = "We couldn't save your profile. Please try again later.";
            ShowEditForm = true;
            await LoadProfileAsync(preserveInput: true);
            return Page();
        }
    }

    private async Task<bool> LoadProfileAsync(bool preserveInput = false)
    {
        Role = HttpContext.Session.GetString("Role") ?? string.Empty;

        try
        {
            var profile = await _data.GetProfileAsync(CurrentUserId);
            if (profile is null)
            {
                return false;
            }

            if (!preserveInput)
            {
                Input.Username = profile.Username;
                Input.Email = profile.Email;
            }

            UserId = profile.UserId;
            Role = profile.Role;
            RegistrationDate = profile.RegistrationDate;
            ProfileAvailable = true;
            return true;
        }
        catch (DbException exception)
        {
            _logger.LogError(exception, "Could not load profile for user {UserId}", CurrentUserId);
            ErrorMessage = "We couldn't load your profile. Please try again later.";
            return true;
        }
        catch (InvalidOperationException exception)
        {
            _logger.LogError(exception, "Could not load profile for user {UserId}", CurrentUserId);
            ErrorMessage = "We couldn't load your profile. Please try again later.";
            return true;
        }
    }
}
