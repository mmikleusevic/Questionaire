using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Web.Interfaces;
using Web.Models;

namespace Web.Components.Pages.Categories.CategoryModals;

public partial class UpdateCategory : ComponentBase
{
    [Inject] ICategoryService? CategoryService { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public Category? Category { get; set; }
    [Parameter] public List<Category>? FlatCategories { get; set; }
    [Parameter] public EventCallback<Category> OnCategoryCreated { get; set; }
    
    private Category? selectedParentCategory;
    private readonly Category updatedCategory = new Category();
    private EditContext? editContext;

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

    private void SelectParentCategory(Category? selectedCategory)
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
        await OnCategoryCreated.InvokeAsync(Category);
        await Hide();
    }

    private async Task Hide()
    {
        if (Modal == null) return;
        
        await Modal.HideAsync();
    }
}