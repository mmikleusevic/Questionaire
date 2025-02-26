using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Services;

public class CategoryService(QuestionaireDbContext context) : ICategoryService
{
    public async Task<CategoriesDto> GetCategories()
    {
        try
        {
            CategoriesDto categoriesDto = new CategoriesDto();

            categoriesDto.NestedCategories = await GetNestedCategories();
            categoriesDto.FlatCategories = await GetFlatCategories();
            
            return categoriesDto;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving categories.", ex);
        }
    }
    
    public async Task<List<CategoryDto>> GetNestedCategories()
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
            throw new InvalidOperationException("An error occurred while retrieving categories.", ex);
        }
    }

    public async Task<List<CategoryDto>> GetFlatCategories()
    {
        try
        {
            List<CategoryDto> categories = await context.Categories
                .OrderBy(c => c.CategoryName)
                .Select(category => new CategoryDto
                {
                    Id = category.Id,
                    CategoryName = category.CategoryName,
                    ParentCategoryId = category.ParentCategoryId,
                    ChildCategories = new List<CategoryDto>()
                })
                .ToListAsync(); 
            
            return categories;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving categories.", ex);
        }
    }
    
    public async Task<Category?> GetCategoryById(int id)
    {
        try
        {
            return await context.Categories.FindAsync(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while retrieving the category with ID {id}.", ex);
        }
    }
    
    public async Task CreateCategory(Category category)
    {
        try
        {
            context.Categories.Add(category);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while creating the category.", ex);
        }
    }

    public async Task<bool> UpdateCategory(int id, Category updatedCategory)
    {
        try
        {
            Category? category = await context.Categories.FirstOrDefaultAsync(a => a.Id == id);

            if (category == null) return false;
            
            category.CategoryName = updatedCategory.CategoryName;
            category.ParentCategoryId = updatedCategory.ParentCategoryId;
            
            await context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while updating the category with ID {id}.", ex);
        }
    }

    public async Task<bool> DeleteCategory(int id)
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
            throw new InvalidOperationException($"An error occurred while deleting the category with ID {id}.", ex);
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
            throw new InvalidOperationException("An error occurred while sorting and mapping categories.", ex);
        }
    }

    private static CategoryDto MapCategoriesToDto(Category category)
    {
        try
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
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while mapping categories.", ex);
        }
    }
}
