using Newtonsoft.Json;

namespace Web.Models;

[JsonObject]
public class CategoryLists
{
    [JsonProperty] public List<Category> NestedCategories { get; set; }

    [JsonProperty] public List<Category> FlatCategories { get; set; }
}