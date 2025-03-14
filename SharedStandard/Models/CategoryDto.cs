using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

        [JsonProperty]
        [Required(ErrorMessage = "Category Name is required")]
        [StringLength(100, ErrorMessage = "Category Name must be between 1 and 100 characters", MinimumLength = 1)]
        public string CategoryName { get; set; }

        [JsonProperty] public int? ParentCategoryId { get; set; }

        [JsonProperty] public List<CategoryDto> ChildCategories { get; set; } = new List<CategoryDto>();

        public bool isSelected { get; set; } = true;
    }
}