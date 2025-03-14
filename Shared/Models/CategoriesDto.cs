using Newtonsoft.Json;

namespace Shared.Models;

[JsonObject]
public class CategoriesDto
{
    [JsonProperty] public List<CategoryDto> NestedCategories { get; set; }
    [JsonProperty] public List<CategoryDto> FlatCategories { get; set; }
}