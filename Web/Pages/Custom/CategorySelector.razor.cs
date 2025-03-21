using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Shared.Models;
using Web.Interfaces;
using Web.Services;

namespace Web.Pages.Custom;

public partial class CategorySelector : ComponentBase
{
    private List<CategoryExtendedDto> searchResults = new List<CategoryExtendedDto>();
    private string searchQuery = string.Empty;
    private int selectedIndex = -1;

    [Parameter] public List<CategoryExtendedDto>? SelectedCategories { get; set; }
    [Inject] private JavaScriptService? JsService { get; set; }
    [Inject] private ICategoryService? CategoryService { get; set; }

    private async Task SearchCategories(string? value)
    {
        searchQuery = value ?? string.Empty;
        selectedIndex = -1;
        
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            if (CategoryService == null) return;
            
            searchResults = await CategoryService.GetFlatCategories(searchQuery);

            if (SelectedCategories == null) return;
            
            searchResults = searchResults.Where(c => !SelectedCategories.Select(sc => sc.Id).Contains(c.Id)).ToList();
        }
        else
        {
            searchResults.Clear();
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "ArrowDown":
                selectedIndex = (selectedIndex + 1) % searchResults.Count;

                if (JsService == null) return;
                
                await JsService.InvokeVoidAsync("scrollToActiveCategory", selectedIndex);
                break;
            case "ArrowUp":
                selectedIndex = (selectedIndex - 1 + searchResults.Count) % searchResults.Count;
                
                if (JsService == null) return;
                
                await JsService.InvokeVoidAsync("scrollToActiveCategory", selectedIndex);
                break;
            case "Enter" when selectedIndex >= 0:
                AddCategoryToSelection(searchResults[selectedIndex]);
                break;
        }
    }

    private void SelectCategory(int index)
    {
        selectedIndex = index;
        AddCategoryToSelection(searchResults[index]);
    }

    private void AddCategoryToSelection(CategoryExtendedDto category)
    {
        if (SelectedCategories != null && !SelectedCategories.Contains(category))
        {
            SelectedCategories.Add(category);
        }
        
        searchResults.Clear();
        searchQuery = string.Empty;
        selectedIndex = -1;
    }

    private void RemoveCategory()
    {
        if (SelectedCategories != null && SelectedCategories.Count != 0)
        {
            SelectedCategories.RemoveAt(SelectedCategories.Count - 1);
        }
    }
}