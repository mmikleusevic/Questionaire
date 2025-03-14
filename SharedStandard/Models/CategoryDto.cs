using System.Collections.Generic;
using Newtonsoft.Json;

namespace SharedStandard.Models
{
    [JsonObject]
    public class CategoryDto
    {
        [JsonProperty] public int Id { get; set; }

        [JsonProperty] public string CategoryName { get; set; }

        [JsonProperty] public int? ParentCategoryId { get; set; }

        [JsonProperty] public List<CategoryDto> ChildCategories { get; set; }

        public bool isSelected { get; set; } = true;
    }
}