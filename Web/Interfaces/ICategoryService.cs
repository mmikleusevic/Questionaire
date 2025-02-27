using Web.Models;
using Categories = Web.Components.Pages.Categories.Categories;

namespace Web.Interfaces;

public interface ICategoryService
{
    Task<CategoryLists> GetCategories(bool forceRefresh = false);
    Task<List<Category>> GetNestedCategories();
    Task<List<Category>> GetFlatCategories();
    Task<Category> GetCategory(int id);
    Task CreateCategory(Category newCategory);
    Task UpdateCategory(Category updatedCategory);
    Task DeleteCategory(int id);
}