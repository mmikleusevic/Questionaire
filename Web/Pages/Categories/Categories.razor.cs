using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using Web.Interfaces;
using Web.Pages.Categories.CategoryModals;

namespace Web.Pages.Categories;

public partial class Categories : ComponentBase
{
    private CategoriesDto? categories;
    private Modal? modal;
    [Inject] private ICategoryService? CategoryService { get; set; }

    private EventCallback OnCategoryChanged => EventCallback.Factory.Create(this, () => GetCategories());

    protected override async Task OnInitializedAsync()
    {
        await GetCategories(true);
    }

    private async Task ShowCreateCategory()
    {
        if (categories == null || modal == null) return;

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "FlatCategories", categories.FlatCategories },
            { "Modal", modal },
            { "OnCategoryChanged", OnCategoryChanged }
        };

        await modal.ShowAsync<CreateCategory>("Create New Category", parameters: parameters);
    }

    private async Task GetCategories(bool forceRefresh = false)
    {
        if (CategoryService == null) return;

        categories = await CategoryService.GetCategories(forceRefresh);
    }
}