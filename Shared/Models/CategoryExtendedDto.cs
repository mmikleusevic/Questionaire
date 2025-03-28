using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Shared.Validation;
using SharedStandard.Models;

namespace Shared.Models;

[JsonObject]
public class CategoryExtendedDto : CategoryDto
{
    public CategoryExtendedDto()
    {
    }

    public CategoryExtendedDto(int id) : base(id)
    {
    }

    [JsonProperty]
    [Required(ErrorMessage = "Category Name is required")]
    [StringLength(100, ErrorMessage = "Category Name must be between 1 and 100 characters", MinimumLength = 1)]
    public override string CategoryName { get; set; }

    [NotOwnParent] public override int? ParentCategoryId { get; set; }

    [ValidateComplexType]
    [JsonProperty]
    public new List<CategoryExtendedDto> ChildCategories { get; set; } = new List<CategoryExtendedDto>();
}