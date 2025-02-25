using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Components.Pages.Categories.CategoryModals;
using Web.Models;

namespace Web.Components.Pages.Categories;

public partial class CategoryItem : ComponentBase
{
    [Parameter] public Category? Category { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public EventCallback OnCategoryChanged { get; set; }
    [Parameter] public List<Category>? FlatCategories { get; set; }
    
    private async Task ShowUpdateCategory()
    {
        if (Modal == null || FlatCategories == null || Category == null) return;
        
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "Modal", Modal },
            { "Category", Category },
            { "FlatCategories", FlatCategories },
            { "OnCategoryChanged", OnCategoryChanged }
        };
        
        await Modal.ShowAsync<UpdateCategory>("Update Category",  parameters: parameters);
    }

    private async Task ShowDeleteCategory()
    {
        if (Modal == null || Category == null) return;
        
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "Modal", Modal },
            { "Category", Category },
            { "OnCategoryChanged", OnCategoryChanged }
        };
        
        await Modal.ShowAsync<DeleteCategory>("Delete Category",  parameters: parameters);
    }
}