using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Web.Interfaces;
using Web.Models;

namespace Web.Components.Pages.Categories;

public partial class CreateCategory : ComponentBase
{
    private Category category = new Category();
    private Category selectedParentCategory;
    
    [Inject] private ICategoryService? CategoryService { get; set; }
    [Parameter] public List<Category>? FlatCategories { get; set; }
    [Parameter] public Modal Modal { get; set; }
    [Parameter] public EventCallback OnCategoryCreated { get; set; }

    private void SelectCategory(Category selectedCategory)
    {
        selectedParentCategory = selectedCategory;
        category.ParentCategoryId = selectedCategory.Id;
    }
    
    public async Task HandleValidSubmit()
    {
        await SaveChanges();
    }
    
    private async Task SaveChanges()
    {
        await CategoryService.CreateCategory(category);
        await Hide();
        await OnCategoryCreated.InvokeAsync();
    }

    private async Task Hide()
    {
        await Modal.HideAsync();
    }
}