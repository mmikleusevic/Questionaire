using Shared.Models;
using SharedStandard.Models;

namespace QuestionaireApi.Interfaces;

public interface ICategoryService
{
    Task<CategoriesDto> GetCategories();
    Task<List<CategoryDto>> GetNestedCategories();
    Task<List<CategoryDto>> GetFlatCategories();
    Task CreateCategory(CategoryDto category);
    Task<bool> UpdateCategory(int id, CategoryDto updatedCategory);
    Task<bool> DeleteCategory(int id);
}