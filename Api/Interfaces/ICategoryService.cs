using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface ICategoryService
{
    Task<List<CategoryDto>> GetCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task CreateCategoryAsync(Category category);
    Task<bool> UpdateCategoryAsync(int id, Category updatedCategory);
    Task<bool> DeleteCategoryAsync(int id);
}