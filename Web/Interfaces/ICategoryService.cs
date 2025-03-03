using Web.Models;
using Categories = Web.Components.Pages.Categories.Categories;

namespace Web.Interfaces;

public interface ICategoryService
{
    Task<CategoryLists> GetCategories(bool forceRefresh = false);
    Task<List<Category>> GetFlatCategories();
    Task CreateCategory(Category newCategory);
    Task UpdateCategory(Category updatedCategory);
    Task DeleteCategory(int id);
}