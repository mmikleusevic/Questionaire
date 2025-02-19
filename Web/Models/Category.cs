using System.Collections;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Web.Models;

[JsonObject]
public class Category
{
    [JsonProperty] 
    public int Id { get; private set; }
    [JsonProperty] 
    [Required(ErrorMessage = "Category Name is required")]
    [StringLength(100, ErrorMessage = "Category Name must be between 1 and 100 characters", MinimumLength = 1)]
    public string CategoryName { get; set; }
    [JsonProperty] 
    public int? ParentCategoryId { get; set; }
    [JsonProperty] 
    public List<Category>? ChildCategories { get; set; } = new List<Category>();
}