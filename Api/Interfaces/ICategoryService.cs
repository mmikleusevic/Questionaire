using QuestionaireApi.Models;

namespace QuestionaireApi.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetCategoriesAsync();
    Task<Category?> GetCategoryByIdAsync(int id);
    Task<HashSet<int>> GetSelectedCategoryIds(List<int> categories);
    Task<Category> AddCategoryAsync(Category category);
    Task<bool> UpdateCategoryAsync(int id, Category updatedCategory);
    Task<bool> DeleteCategoryAsync(int id);
}