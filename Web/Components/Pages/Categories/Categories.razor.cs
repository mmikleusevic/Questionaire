using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Components.Pages.Categories.CategoryModals;
using Web.Interfaces;
using Web.Models;

namespace Web.Components.Pages.Categories;

public partial class Categories : ComponentBase
{
    [Inject] private ICategoryService? CategoryService { get; set; }

    private EventCallback OnCategoryChanged => EventCallback.Factory.Create(this, () => GetCategories());
    private Modal? modal;
    private CategoryLists? categories;
    
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