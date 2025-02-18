using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Interfaces;
using Web.Models;

namespace Web.Components.Pages.Categories.CategoryModals;

public partial class DeleteCategory : ComponentBase
{
    [Inject] ICategoryService CategoryService { get; set; }
    [Parameter] public Modal Modal { get; set; }
    [Parameter] public Category Category { get; set; }
    [Parameter] public EventCallback<Category> OnCategoryCreated { get; set; }
    
    private async Task HandleValidSubmit()
    {
        await CategoryService.DeleteCategory(Category.Id);
        await Hide();
        await OnCategoryCreated.InvokeAsync(Category);
    }

    private async Task Hide()
    {
        await Modal.HideAsync();
    }
}