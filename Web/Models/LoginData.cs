using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class LoginData
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;
    [PasswordPropertyText]
    [Required(ErrorMessage = "Password is required")]
    [StringLength(24, MinimumLength = 6, ErrorMessage = "The password must contain at least 6 and max 24 characters")]
    public string Password { get; set; } = string.Empty;
}