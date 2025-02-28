using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface ICategoryService
{
    Task<CategoriesDto> GetCategories();
    Task<List<CategoryDto>> GetNestedCategories();
    Task<List<CategoryDto>> GetFlatCategories();
    Task<Category?> GetCategoryById(int id);
    Task CreateCategory(CategoryDto category);
    Task<bool> UpdateCategory(int id, CategoryDto updatedCategory);
    Task<bool> DeleteCategory(int id);
}