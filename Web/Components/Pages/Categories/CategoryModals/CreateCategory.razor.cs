using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Interfaces;
using Web.Models;

namespace Web.Components.Pages.Categories.CategoryModals;

public partial class CreateCategory : ComponentBase
{
    [Inject] private ICategoryService? CategoryService { get; set; }
    [Parameter] public List<Category>? FlatCategories { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public EventCallback OnCategoryCreated { get; set; }

    private readonly Category category = new Category();
    private Category? selectedParentCategory;
    
    protected override async Task OnParametersSetAsync()
    {
        category.CategoryName = string.Empty;
        selectedParentCategory = null;
        
        await base.OnParametersSetAsync();
    }
    
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
        if (CategoryService == null) return;
        
        await CategoryService.CreateCategory(category);
        await OnCategoryCreated.InvokeAsync();
        await Hide();
    }

    private async Task Hide()
    {
        if (Modal == null) return;
        
        await Modal.HideAsync();
    }
}