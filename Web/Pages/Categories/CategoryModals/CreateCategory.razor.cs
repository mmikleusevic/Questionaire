using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using SharedStandard.Models;
using Web.Interfaces;

namespace Web.Pages.Categories.CategoryModals;

public partial class CreateCategory : ComponentBase
{
    private readonly CategoryDto category = new CategoryDto();
    private EditContext? editContext;
    private CategoryDto? selectedParentCategory;
    [Inject] private ICategoryService? CategoryService { get; set; }
    [Parameter] public List<CategoryDto>? FlatCategories { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public EventCallback OnCategoryChanged { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        category.CategoryName = string.Empty;
        selectedParentCategory = null;

        editContext = new EditContext(category);

        await base.OnParametersSetAsync();
    }

    private void SelectParentCategory(CategoryDto? selectedCategory)
    {
        selectedParentCategory = selectedCategory;

        if (selectedCategory == null)
        {
            category.ParentCategoryId = null;
        }
        else
        {
            category.ParentCategoryId = selectedCategory.Id;
        }
    }

    public async Task HandleValidSubmit()
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