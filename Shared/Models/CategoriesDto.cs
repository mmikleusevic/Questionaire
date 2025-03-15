using Newtonsoft.Json;

namespace Shared.Models;

[JsonObject]
public class CategoriesDto
{
    [JsonProperty] public List<CategoryValidationDto> NestedCategories { get; set; }
    [JsonProperty] public List<CategoryValidationDto> FlatCategories { get; set; }
}