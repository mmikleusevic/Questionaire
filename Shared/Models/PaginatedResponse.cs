using Newtonsoft.Json;

namespace Shared.Models;

[JsonObject]
public class PaginatedResponse<T>
{
    [JsonProperty] public List<T> Items { get; set; } = new List<T>();

    [JsonProperty] public int TotalCount { get; set; }

    [JsonProperty] public int PageSize { get; set; }

    [JsonProperty] public int TotalPages { get; set; }
}