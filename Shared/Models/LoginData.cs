using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Shared.Models;

public class LoginData
{
    [Required(ErrorMessage = "UserName is required")]
    public string UserName { get; set; } = string.Empty;

    [PasswordPropertyText]
    [Required(ErrorMessage = "Password is required")]
    [StringLength(24, MinimumLength = 6, ErrorMessage = "The password must contain at least 6 and max 24 characters")]
    public string Password { get; set; } = string.Empty;
}