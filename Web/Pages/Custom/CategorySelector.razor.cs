using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Shared.Models;
using Web.Interfaces;
using Web.Services;

namespace Web.Pages.Custom;

public partial class CategorySelector : ComponentBase
{
    private ElementReference inputElement;
    private string searchQuery = string.Empty;
    private List<CategoryExtendedDto> searchResults = new List<CategoryExtendedDto>();
    private int selectedIndex = -1;
    private bool showDropdown = false;

    [Parameter] public List<CategoryExtendedDto>? SelectedCategories { get; set; }
    [Inject] private JavaScriptService? JsService { get; set; }
    [Inject] private ICategoryService? CategoryService { get; set; }

    private async Task SearchCategories(string? value)
    {
        searchQuery = value ?? string.Empty;
        selectedIndex = -1;

        if (CategoryService == null) return;

        searchResults = await CategoryService.GetFlatCategories(searchQuery);

        if (SelectedCategories == null) return;

        searchResults = searchResults.Where(c => !SelectedCategories.Select(sc => sc.Id).Contains(c.Id)).ToList();
    }

    private async Task OnFocus()
    {
        ShowDropdown();
        await SearchCategories(searchQuery);
    }

    private void ShowDropdown()
    {
        showDropdown = true;
    }

    private void HideDropdown()
    {
        showDropdown = false;
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "ArrowDown":
            {
                if (searchResults.Count == 0) return;

                int possibleIndex = (selectedIndex + 1) % searchResults.Count;
                if (possibleIndex >= 0)
                {
                    selectedIndex = possibleIndex;
                }

                if (JsService == null) return;

                await JsService.InvokeVoidAsync("scrollToActiveCategory", selectedIndex);
                break;
            }
            case "ArrowUp":
            {
                if (searchResults.Count == 0) return;

                int possibleIndex = (selectedIndex - 1 + searchResults.Count) % searchResults.Count;
                if (possibleIndex >= 0)
                {
                    selectedIndex = possibleIndex;
                }

                if (JsService == null) return;

                await JsService.InvokeVoidAsync("scrollToActiveCategory", selectedIndex);
                break;
            }
            case "Enter" when selectedIndex >= 0:
                AddCategoryToSelection(searchResults[selectedIndex]);

                if (JsService == null) return;

                await JsService.InvokeVoidAsync("blurElement", inputElement);
                break;
        }
    }

    private async Task SelectCategory(int index)
    {
        selectedIndex = index;
        AddCategoryToSelection(searchResults[index]);

        if (JsService == null) return;

        await JsService.InvokeVoidAsync("blurElement", inputElement);
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