using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Shared.Models;

[JsonObject]
public class UserDto
{
    [JsonProperty] [Required] public string Email { get; set; } = string.Empty;
    [JsonProperty] [Required] public IList<string> Roles { get; set; } = new List<string>();
}