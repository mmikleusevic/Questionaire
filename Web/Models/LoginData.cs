using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class LoginData
{
    [EmailAddress]
    [Required(ErrorMessage = "Email is required")]
    public string Email { get; set; } = string.Empty;

    [PasswordPropertyText]
    [Required(ErrorMessage = "Password is required")]
    [StringLength(24, MinimumLength = 6, ErrorMessage = "The password must contain at least 6 and max 24 characters")]
    public string Password { get; set; } = string.Empty;
}