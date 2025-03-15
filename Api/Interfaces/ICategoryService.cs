using Shared.Models;

namespace QuestionaireApi.Interfaces;

public interface ICategoryService
{
    Task<CategoriesDto> GetCategories();
    Task<List<CategoryValidationDto>> GetNestedCategories();
    Task<List<CategoryValidationDto>> GetFlatCategories();
    Task CreateCategory(CategoryValidationDto category);
    Task<bool> UpdateCategory(int id, CategoryValidationDto updatedCategory);
    Task<bool> DeleteCategory(int id);
}