using Web.Models;

namespace Web.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetCategories();
    Task<Category> GetCategory(int id);
    Task CreateCategory(Category category);
    Task UpdateCategory(Category category, int id);
    Task DeleteCategory(int id);
}