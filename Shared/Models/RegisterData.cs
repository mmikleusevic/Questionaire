using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models;

public class RegisterData : LoginData
{
    [PasswordPropertyText]
    [Required(ErrorMessage = "Confirm Password is required")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
    [StringLength(24, MinimumLength = 6,
        ErrorMessage = "The confirm password must contain at least 6 and max 24 characters")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [EmailAddress]
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = string.Empty;
}