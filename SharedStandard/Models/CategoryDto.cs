using System.Collections.Generic;
using Newtonsoft.Json;

namespace SharedStandard.Models
{
    [JsonObject]
    public class CategoryDto
    {
        public CategoryDto()
        {
        }

        public CategoryDto(int id)
        {
            Id = id;
        }

        [JsonProperty] public int Id { get; private set; }
        [JsonProperty] public virtual string CategoryName { get; set; }
        [JsonProperty] public virtual int? ParentCategoryId { get; set; }
        [JsonProperty] public List<CategoryDto> ChildCategories { get; set; } = new List<CategoryDto>();
        public bool isSelected { get; set; } = true;
    }
}