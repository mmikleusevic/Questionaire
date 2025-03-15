using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Shared.Models;

[JsonObject]
public class UserDto(string email, IList<string> roles)
{
    [JsonProperty] [Required] public string? Email { get; private set; } = email;
    [JsonProperty] [Required] public IList<string>? Roles { get; private set; } = roles;
}