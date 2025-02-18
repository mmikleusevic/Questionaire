using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Components.Pages.Categories.CategoryModals;
using Web.Models;

namespace Web.Components.Pages.Categories;

public partial class CategoryItem : ComponentBase
{
    [Parameter] public Category Category { get; set; }
    [Parameter] public Modal Modal { get; set; }
    [Parameter] public EventCallback<Category> OnCategoryCreated { get; set; }
    
    private void UpdateCategory()
    {
        Console.WriteLine($"Update category: {Category.CategoryName}");
    }

    private async Task DeleteCategory()
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "Modal", Modal },
            { "Category", Category },
            { "OnCategoryCreated", OnCategoryCreated }
        };
        
        await Modal.ShowAsync<DeleteCategory>($"Delete \"{Category.CategoryName}\" Category",  parameters: parameters);
    }
}