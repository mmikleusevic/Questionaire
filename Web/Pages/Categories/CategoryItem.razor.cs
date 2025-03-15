using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using Web.Pages.Categories.CategoryModals;
using UpdateCategory = Web.Pages.Categories.CategoryModals.UpdateCategory;

namespace Web.Pages.Categories;

public partial class CategoryItem : ComponentBase
{
    [Parameter] public CategoryValidationDto? Category { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public EventCallback OnCategoryChanged { get; set; }
    [Parameter] public List<CategoryValidationDto>? FlatCategories { get; set; }

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

        await Modal.ShowAsync<UpdateCategory>("Update Category", parameters: parameters);
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

        await Modal.ShowAsync<DeleteCategory>("Delete Category", parameters: parameters);
    }
}