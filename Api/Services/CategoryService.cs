using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using Shared.Models;

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

    public async Task<List<CategoryValidationDto>> GetNestedCategories()
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

    public async Task<List<CategoryValidationDto>> GetFlatCategories()
    {
        try
        {
            List<CategoryValidationDto> categories = await context.Categories
                .OrderBy(c => c.CategoryName)
                .Select(category => new CategoryValidationDto(category.Id)
                {
                    CategoryName = category.CategoryName,
                    ParentCategoryId = category.ParentCategoryId
                })
                .ToListAsync();

            return categories;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving categories.", ex);
        }
    }

    public async Task CreateCategory(CategoryValidationDto category)
    {
        try
        {
            await context.Categories.AddAsync(new Category
            {
                CategoryName = category.CategoryName,
                ParentCategoryId = category.ParentCategoryId
            });

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while creating the category.", ex);
        }
    }

    public async Task<bool> UpdateCategory(int id, CategoryValidationDto updatedCategory)
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

    private List<CategoryValidationDto> SortAndMapCategories(IEnumerable<Category> categories)
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

    private static CategoryValidationDto MapCategoriesToDto(Category category)
    {
        try
        {
            return new CategoryValidationDto(category.Id)
            {
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