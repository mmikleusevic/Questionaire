using Newtonsoft.Json;

namespace Shared.Models;

[JsonObject]
public class CategoriesDto
{
    [JsonProperty] public List<CategoryExtendedDto> NestedCategories { get; set; }
    [JsonProperty] public List<CategoryExtendedDto> FlatCategories { get; set; }
}