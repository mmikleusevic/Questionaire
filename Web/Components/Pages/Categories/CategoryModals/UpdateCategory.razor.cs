using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Interfaces;
using Web.Models;

namespace Web.Components.Pages.Categories.CategoryModals;

public partial class UpdateCategory : ComponentBase
{
    [Inject] ICategoryService CategoryService { get; set; }
    [Parameter] public Modal Modal { get; set; }
    [Parameter] public Category Category { get; set; }
    [Parameter] public List<Category>? FlatCategories { get; set; }
    [Parameter] public EventCallback<Category> OnCategoryCreated { get; set; }
    
    private Category? selectedParentCategory;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        
        selectedParentCategory = FlatCategories?.FirstOrDefault(c => c.Id == Category.ParentCategoryId);
    }

    private void SelectParentCategory(Category? selectedCategory)
    {
        selectedParentCategory = selectedCategory;

        if (selectedCategory == null)
        {
            Category.ParentCategoryId = null;
        }
        else
        {
            Category.ParentCategoryId = selectedCategory.Id;
        }
    }
    
    private async Task HandleValidSubmit()
    {
        await CategoryService.UpdateCategory(Category);
        await Hide();
        await OnCategoryCreated.InvokeAsync(Category);
    }

    private async Task Hide()
    {
        selectedParentCategory = null;
        await Modal.HideAsync();
    }
}