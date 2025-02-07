using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;

namespace QuestionaireApi.Services;

public class CategoryService(QuestionaireDbContext context) : ICategoryService
{
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.ChildCategories)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await context.Categories.FindAsync(id);
    }

    public async Task<HashSet<int>> GetSelectedCategoryIds(List<int> categoryIds)
    {
        HashSet<int> processedIds = new HashSet<int>(categoryIds);

        async Task GetChildCategoryIds(List<int> ids)
        {
            List<int> newChildIds = (await context.Categories
                    .Where(c => ids.Contains(c.Id))
                    .Include(c => c.ChildCategories)
                    .ToListAsync())
                .SelectMany(c => c.ChildCategories)
                .Select(cc => cc.Id)
                .Where(id => processedIds.Add(id))
                .ToList();

            if (newChildIds.Count > 0)
            {
                await GetChildCategoryIds(newChildIds);
            }
        }
        
        await GetChildCategoryIds(categoryIds);
        
        return processedIds;
    }
    
    public async Task<Category> AddCategoryAsync(Category category)
    {
        context.Categories.Add(category);
        await context.SaveChangesAsync();
        return category;
    }

    public async Task<bool> UpdateCategoryAsync(int id, Category updatedCategory)
    {
        Category category = await context.Categories.FindAsync(id);
        if (category == null) return false;

        category.CategoryName = updatedCategory.CategoryName;
        
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        Category category = await context.Categories.FindAsync(id);
        if (category == null) return false;

        context.Categories.Remove(category);
        await context.SaveChangesAsync();
        return true;
    }
}