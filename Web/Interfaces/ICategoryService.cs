using Shared.Models;

namespace Web.Interfaces;

public interface ICategoryService
{
    Task<CategoriesDto> GetCategories(bool forceRefresh = false);
    Task<List<CategoryDto>> GetNestedCategories();
    Task<List<CategoryDto>> GetFlatCategories();
    Task CreateCategory(CategoryDto newCategory);
    Task UpdateCategory(CategoryDto updatedCategory);
    Task DeleteCategory(int id);
}