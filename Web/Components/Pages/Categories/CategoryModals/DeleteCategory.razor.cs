using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Interfaces;
using Web.Models;

namespace Web.Components.Pages.Categories.CategoryModals;

public partial class DeleteCategory : ComponentBase
{
    [Inject] ICategoryService? CategoryService { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public Category? Category { get; set; }
    [Parameter] public EventCallback<Category> OnCategoryCreated { get; set; }
    
    private async Task HandleValidSubmit()
    {
        if (Category == null || CategoryService == null) return;
        
        await CategoryService.DeleteCategory(Category.Id);
        await OnCategoryCreated.InvokeAsync(Category);
        await Hide();
    }

    private async Task Hide()
    {
        if (Modal == null) return;
        
        await Modal.HideAsync();
    }
}