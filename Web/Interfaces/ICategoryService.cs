using Web.Models;

namespace Web.Interfaces;

public interface ICategoryService
{
    Task<List<Category>> GetCategories();
    Task<List<Category>> GetFlatCategories();
    Task<Category> GetCategory(int id);
    Task CreateCategory(Category newCategory);
    Task UpdateCategory(Category updatedCategory);
    Task DeleteCategory(int id);
}