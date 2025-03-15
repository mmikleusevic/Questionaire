using System.ComponentModel.DataAnnotations;
using SharedStandard.Models;

namespace Shared.Models;

public class CategoryValidationDto : CategoryDto
{
    public CategoryValidationDto()
    {
    }

    public CategoryValidationDto(int id) : base(id)
    {
    }

    [Required(ErrorMessage = "Category Name is required")]
    [StringLength(100, ErrorMessage = "Category Name must be between 1 and 100 characters", MinimumLength = 1)]
    public new string CategoryName { get; set; }

    [ValidateComplexType] public new List<CategoryValidationDto> ChildCategories { get; set; } = new List<CategoryValidationDto>();
}