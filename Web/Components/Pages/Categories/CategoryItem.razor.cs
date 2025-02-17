using Microsoft.AspNetCore.Components;
using Web.Models;

namespace Web.Components.Pages.Categories;

public partial class CategoryItem : ComponentBase
{
    [Parameter] public Category Category { get; set; }
    
    private void UpdateCategory()
    {
        Console.WriteLine($"Update category: {Category.CategoryName}");
    }

    private void RemoveCategory()
    {
        Console.WriteLine($"Remove category: {Category.CategoryName}");
    }
}