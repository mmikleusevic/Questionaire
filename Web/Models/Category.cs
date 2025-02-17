using System.Collections;
using Newtonsoft.Json;

namespace Web.Models;

[JsonObject]
public class Category
{
    [JsonProperty] 
    public int Id { get; set; }
    [JsonProperty] 
    public string CategoryName { get; set; }
    [JsonProperty] 
    public int? ParentCategoryId { get; set; }
    [JsonProperty] 
    public List<Category>? ChildCategories { get; set; }
}