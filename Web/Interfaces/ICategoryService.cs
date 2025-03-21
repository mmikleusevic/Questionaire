using Shared.Models;

namespace Web.Interfaces;

public interface ICategoryService
{
    Task<CategoriesDto> GetCategories(bool forceRefresh = false);
    Task<List<CategoryExtendedDto>> GetNestedCategories();
    Task<List<CategoryExtendedDto>> GetFlatCategories(string searchQuery = "");
    Task CreateCategory(CategoryExtendedDto newCategory);
    Task UpdateCategory(CategoryExtendedDto updatedCategory);
    Task DeleteCategory(int id);
}