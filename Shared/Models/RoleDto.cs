using Newtonsoft.Json;

namespace Shared.Models;

[JsonObject]
public class RoleDto
{
    [JsonProperty]
    public string RoleName { get; set; }
}