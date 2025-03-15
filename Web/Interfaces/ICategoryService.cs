using Shared.Models;

namespace Web.Interfaces;

public interface ICategoryService
{
    Task<CategoriesDto> GetCategories(bool forceRefresh = false);
    Task<List<CategoryValidationDto>> GetNestedCategories();
    Task<List<CategoryValidationDto>> GetFlatCategories();
    Task CreateCategory(CategoryValidationDto newCategory);
    Task UpdateCategory(CategoryValidationDto updatedCategory);
    Task DeleteCategory(int id);
}