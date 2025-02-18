using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Web.Components.Pages.Categories.CategoryModals;
using Web.Interfaces;
using Web.Models;

namespace Web.Components.Pages.Categories;

public partial class Categories : ComponentBase
{
    [Inject] private ICategoryService? CategoryService { get; set; }

    private EventCallback onCategoryUpdated => EventCallback.Factory.Create(this, GetCategories);
    private Modal modal = default!;
    private List<Category>? categories;
    private List<Category> flatCategories;
    
    protected override async Task OnInitializedAsync()
    {
        await GetCategories();
    }
    
    private async Task OpenCreateCategoryModal()
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "FlatCategories", flatCategories },
            { "Modal", modal },
            { "OnCategoryCreated", EventCallback.Factory.Create(this, GetCategories) }
        };
        
        await modal.ShowAsync<CreateCategory>(title: "Create New Category", parameters: parameters);
    }

    private async Task GetCategories()
    {
        categories = await CategoryService.GetCategories();

        if (categories != null) flatCategories = GetFlatCategories(categories);
    }

    private List<Category> GetFlatCategories(List<Category> categories)
    {
        return categories.SelectMany(c => new[] { c }
            .Concat(GetFlatCategories(c.ChildCategories ?? new List<Category>()))).ToList();
    }
}