using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Services;

public class CategoryService(QuestionaireDbContext context) : ICategoryService
{
    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        try
        {
            List<Category> categories = await context.Categories
                .Include(c => c.ParentCategory)
                .Include(c => c.ChildCategories)
                .OrderBy(c => c.CategoryName)
                .ToListAsync(); 

            return SortAndMapCategories(categories);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while retrieving categories.", ex);
        }
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        try
        {
            return await context.Categories.FindAsync(id);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while retrieving the category with ID {id}.", ex);
        }
    }
    
    public async Task CreateCategoryAsync(Category category)
    {
        try
        {
            context.Categories.Add(category);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while creating the category.", ex);
        }
    }

    public async Task<bool> UpdateCategoryAsync(int id, Category updatedCategory)
    {
        try
        {
            Category? category = await context.Categories.FirstOrDefaultAsync(a =>a.Id == id);

            if (category == null) return false;
            
            category.CategoryName = updatedCategory.CategoryName;
            
            await context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while updating the category with ID {id}.", ex);
        }
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        try
        {
            Category? category = await context.Categories.FirstOrDefaultAsync(a => a.Id == id);
            if (category == null) return false;

            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while deleting the category with ID {id}.", ex);
        }
    }
    
    private List<CategoryDto> SortAndMapCategories(IEnumerable<Category> categories)
    {
        try
        {
            return categories
                .Where(c => c.ParentCategory == null)
                .SelectMany(c => new[] { MapCategoriesToDto(c) }.Concat(SortAndMapCategories(c.ChildCategories)))
                .ToList();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while sorting and mapping categories.", ex);
        }
    }

    private static CategoryDto MapCategoriesToDto(Category category)
    {
        return new CategoryDto
        {
            Id = category.Id,
            CategoryName = category.CategoryName,
            ParentCategoryId = category.ParentCategoryId,
            ChildCategories = category.ChildCategories
                .Select(MapCategoriesToDto)
                .ToList()
        };
    }
}
