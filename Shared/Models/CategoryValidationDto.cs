using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using SharedStandard.Models;

namespace Shared.Models;

[JsonObject]
public class CategoryValidationDto : CategoryDto
{
    public CategoryValidationDto()
    {
    }

    public CategoryValidationDto(int id) : base(id)
    {
    }

    [JsonProperty]
    [Required(ErrorMessage = "Category Name is required")]
    [StringLength(100, ErrorMessage = "Category Name must be between 1 and 100 characters", MinimumLength = 1)]
    public new string CategoryName { get; set; }

    [JsonProperty] public string ParentCategoryName { get; set; }

    [ValidateComplexType]
    [JsonProperty]
    public new List<CategoryValidationDto> ChildCategories { get; set; } = new List<CategoryValidationDto>();
}