using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using Web.Interfaces;

namespace Web.Pages.Categories.CategoryModals;

public partial class DeleteCategory : ComponentBase
{
    [Inject] private ICategoryService? CategoryService { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public CategoryExtendedDto? Category { get; set; }
    [Parameter] public EventCallback OnCategoryChanged { get; set; }

    private async Task Delete()
    {
        if (CategoryService == null) return;

        await CategoryService.DeleteCategory(Category.Id);
    }
}