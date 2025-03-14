using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using SharedStandard.Models;
using Web.Interfaces;

namespace Web.Pages.Categories.CategoryModals;

public partial class DeleteCategory : ComponentBase
{
    [Inject] private ICategoryService? CategoryService { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public CategoryDto? Category { get; set; }
    [Parameter] public EventCallback OnCategoryChanged { get; set; }

    private async Task HandleValidSubmit()
    {
        if (Category == null || CategoryService == null) return;

        await CategoryService.DeleteCategory(Category.Id);
        await OnCategoryChanged.InvokeAsync(Category);
        await Hide();
    }

    private async Task Hide()
    {
        if (Modal == null) return;

        await Modal.HideAsync();
    }
}