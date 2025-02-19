using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Components.Pages.Categories.CategoryModals;
using Web.Models;

namespace Web.Components.Pages.Categories;

public partial class CategoryItem : ComponentBase
{
    [Parameter] public Category? Category { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public EventCallback<Category> OnCategoryCreated { get; set; }
    [Parameter] public List<Category>? FlatCategories { get; set; }
    
    private async Task UpdateCategory()
    {
        if (Modal == null || FlatCategories == null || Category == null) return;
        
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "Modal", Modal },
            { "Category", Category },
            { "FlatCategories", FlatCategories },
            { "OnCategoryCreated", OnCategoryCreated }
        };
        
        await Modal.ShowAsync<UpdateCategory>($"Update \"{Category.CategoryName}\" Category",  parameters: parameters);
    }

    private async Task DeleteCategory()
    {
        if (Modal == null || Category == null) return;
        
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "Modal", Modal },
            { "Category", Category },
            { "OnCategoryCreated", OnCategoryCreated }
        };
        
        await Modal.ShowAsync<DeleteCategory>($"Delete \"{Category.CategoryName}\" Category",  parameters: parameters);
    }
}