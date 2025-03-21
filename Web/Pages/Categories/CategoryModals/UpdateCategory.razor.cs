using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Models;
using Web.Interfaces;

namespace Web.Pages.Categories.CategoryModals;

public partial class UpdateCategory : ComponentBase
{
    private readonly CategoryExtendedDto updatedCategory = new CategoryExtendedDto();
    private EditContext? editContext;

    private CategoryExtendedDto? selectedParentCategory;
    [Inject] private ICategoryService? CategoryService { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public CategoryExtendedDto? Category { get; set; }
    [Parameter] public List<CategoryExtendedDto>? FlatCategories { get; set; }
    [Parameter] public EventCallback OnCategoryChanged { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (Category == null) return;

        updatedCategory.ParentCategoryId = Category.ParentCategoryId;
        updatedCategory.CategoryName = Category.CategoryName;

        SelectDefaultParentCategory();

        editContext = new EditContext(updatedCategory);

        await base.OnParametersSetAsync();
    }

    private void SelectDefaultParentCategory()
    {
        if (Category == null) return;

        selectedParentCategory = FlatCategories?.FirstOrDefault(c => c.Id == Category.ParentCategoryId);
        SelectParentCategory(selectedParentCategory);
    }

    private void SelectParentCategory(CategoryExtendedDto? selectedCategory)
    {
        selectedParentCategory = selectedCategory;

        if (selectedCategory == null)
        {
            updatedCategory.ParentCategoryId = null;
        }
        else
        {
            updatedCategory.ParentCategoryId = selectedCategory.Id;
        }
    }

    private async Task HandleValidSubmit()
    {
        if (Category == null || CategoryService == null) return;

        Category.ParentCategoryId = updatedCategory.ParentCategoryId;
        Category.CategoryName = updatedCategory.CategoryName;

        await CategoryService.UpdateCategory(Category);
        await OnCategoryChanged.InvokeAsync(Category);
        await Hide();
    }

    private async Task Hide()
    {
        if (Modal == null) return;

        await Modal.HideAsync();
    }
}