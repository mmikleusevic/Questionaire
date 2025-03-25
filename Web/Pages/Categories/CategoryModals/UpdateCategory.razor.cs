using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using Web.Interfaces;

namespace Web.Pages.Categories.CategoryModals;

public partial class UpdateCategory : ComponentBase
{
    private CategoryExtendedDto? updatedCategory;
    [Inject] private ICategoryService? CategoryService { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public CategoryExtendedDto? Category { get; set; }
    [Parameter] public List<CategoryExtendedDto>? FlatCategories { get; set; }
    [Parameter] public EventCallback OnCategoryChanged { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (Category == null) return;

        updatedCategory = new CategoryExtendedDto(Category.Id)
        {
            ParentCategoryId = Category.ParentCategoryId,
            CategoryName = Category.CategoryName
        };

        await base.OnParametersSetAsync();
    }

    private async Task UpdateExistingCategory(CategoryExtendedDto category)
    {
        if (CategoryService == null || Category == null) return;

        Category.ParentCategoryId = category.ParentCategoryId;
        Category.CategoryName = category.CategoryName;

        await CategoryService.UpdateCategory(Category);
        await OnCategoryChanged.InvokeAsync();
        await Hide();
    }

    private async Task Hide()
    {
        if (Modal == null) return;

        await Modal.HideAsync();
    }
}