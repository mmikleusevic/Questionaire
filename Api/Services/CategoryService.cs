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

    public async Task<List<CategoryExtendedDto>> GetNestedCategories()
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

    public async Task<List<CategoryExtendedDto>> GetFlatCategories()
    {
        try
        {
            List<CategoryExtendedDto> allCategories = await context.Categories
                .Include(c => c.ParentCategory)
                .Select(category => new CategoryExtendedDto(category.Id)
                {
                    CategoryName = category.CategoryName,
                    ParentCategoryId = category.ParentCategoryId,
                    ParentCategoryName = category.ParentCategory != null
                        ? category.ParentCategory.CategoryName
                        : string.Empty
                })
                .ToListAsync();

            Dictionary<int, List<CategoryExtendedDto>> categoryChildrenDict = allCategories
                .GroupBy(c => c.ParentCategoryId ?? -1)
                .ToDictionary(g => g.Key, g => g.OrderBy(c => c.CategoryName).ToList());

            List<CategoryExtendedDto> result = new List<CategoryExtendedDto>();

            AddDescendantsInOrder(-1, categoryChildrenDict, result);

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving categories.", ex);
        }
    }

    public async Task CreateCategory(CategoryExtendedDto category)
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

    public async Task<bool> UpdateCategory(int id, CategoryExtendedDto updatedCategory)
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

    private List<CategoryExtendedDto> SortAndMapCategories(IEnumerable<Category> categories)
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

    private static CategoryExtendedDto MapCategoriesToDto(Category category)
    {
        try
        {
            return new CategoryExtendedDto(category.Id)
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

    private void AddDescendantsInOrder(int parentId, Dictionary<int, List<CategoryExtendedDto>> categoryChildrenDict,
        List<CategoryExtendedDto> result)
    {
        try
        {
            if (categoryChildrenDict.TryGetValue(parentId, out List<CategoryExtendedDto>? children))
            {
                foreach (CategoryExtendedDto child in children)
                {
                    result.Add(child);
                    AddDescendantsInOrder(child.Id, categoryChildrenDict, result);
                }
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while adding sub categories.", ex);
        }
    }
}