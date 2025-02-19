using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Interfaces;
using Web.Models;

namespace Web.Components.Pages.Categories.CategoryModals;

public partial class CreateCategory : ComponentBase
{
    [Inject] private ICategoryService? CategoryService { get; set; }
    [Parameter] public List<Category>? FlatCategories { get; set; }
    [Parameter] public Modal Modal { get; set; }
    [Parameter] public EventCallback OnCategoryCreated { get; set; }

    private Category category = new Category();
    private Category selectedParentCategory;
    
    private void SelectParentCategory(Category? selectedCategory)
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
        await CategoryService.CreateCategory(category);
        await Hide();
        await OnCategoryCreated.InvokeAsync();
    }

    private async Task Hide()
    {
        selectedParentCategory = null;
        await Modal.HideAsync();
    }
}