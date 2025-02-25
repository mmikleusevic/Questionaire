using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Components.Pages.Categories.CategoryModals;
using Web.Interfaces;
using Web.Models;

namespace Web.Components.Pages.Categories;

public partial class Categories : ComponentBase
{
    [Inject] private ICategoryService? CategoryService { get; set; }

    private EventCallback OnCategoryUpdated => EventCallback.Factory.Create(this, GetCategories);
    private Modal modal = null!;
    private CategoryLists categories;
    
    protected override async Task OnInitializedAsync()
    {
        await GetCategories();
    }
    
    private async Task ShowCreateCategory()
    {
        if (categories == null) return;
        
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "FlatCategories", categories.FlatCategories },
            { "Modal", modal },
            { "OnCategoryCreated", EventCallback.Factory.Create(this, GetCategories) }
        };
        
        await modal.ShowAsync<CreateCategory>(title: "Create New Category", parameters: parameters);
    }

    private async Task GetCategories()
    {
        if (CategoryService == null) return;
        
        categories = await CategoryService.GetCategories();
    }
}