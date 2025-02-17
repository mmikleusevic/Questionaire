using Microsoft.AspNetCore.Components;
using Web.Interfaces;
using Web.Models;

namespace Web.Components.Pages.Categories;

public partial class Categories : ComponentBase
{
    [Inject] private ICategoryService? CategoryService { get; set; } = null!;
    
    private List<Category>? categories;
    
    protected override async Task OnInitializedAsync()
    {
        categories = await CategoryService.GetCategories();
    }
    
    private void CreateCategory()
    {
        Console.WriteLine($"Create category");
    }
}