using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using Web.Interfaces;

namespace Web.Pages.Categories.CategoryModals;

public partial class CreateCategory : ComponentBase
{
    private CategoryExtendedDto? category;
    [Inject] private ICategoryService? CategoryService { get; set; }
    [Parameter] public List<CategoryExtendedDto>? FlatCategories { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public EventCallback OnCategoryChanged { get; set; }

    protected override void OnParametersSet()
    {
        category = new CategoryExtendedDto();
    }

    private async Task CreateNewCategory(CategoryExtendedDto category)
    {
        if (CategoryService == null) return;

        await CategoryService.CreateCategory(category);
        await OnCategoryChanged.InvokeAsync();
        await Hide();
    }

    private async Task Hide()
    {
        if (Modal == null) return;

        await Modal.HideAsync();
    }
}