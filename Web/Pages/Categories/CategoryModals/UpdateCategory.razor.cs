using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using Web.Interfaces;

namespace Web.Pages.Categories.CategoryModals;

public partial class UpdateCategory : ComponentBase
{
    [Inject] private ICategoryService? CategoryService { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public CategoryExtendedDto? Category { get; set; }
    [Parameter] public List<CategoryExtendedDto>? FlatCategories { get; set; }
    [Parameter] public EventCallback OnCategoryChanged { get; set; }

    private async Task UpdateExistingCategory(CategoryExtendedDto category)
    {
        if (CategoryService == null || Category == null) return;

        await CategoryService.UpdateCategory(category);
        await OnCategoryChanged.InvokeAsync();
        await Hide();
    }

    private async Task Hide()
    {
        if (Modal == null) return;

        await Modal.HideAsync();
    }
}