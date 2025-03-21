using Shared.Models;

namespace QuestionaireApi.Interfaces;

public interface ICategoryService
{
    Task<CategoriesDto> GetCategories();
    Task<List<CategoryExtendedDto>> GetNestedCategories();
    Task<List<CategoryExtendedDto>> GetFlatCategories(string searchQuery);
    Task CreateCategory(CategoryExtendedDto category);
    Task<bool> UpdateCategory(int id, CategoryExtendedDto updatedCategory);
    Task<bool> DeleteCategory(int id);
}