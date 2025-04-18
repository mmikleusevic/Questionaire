using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Shared.Models;
using Web.Handlers;
using Web.Interfaces;

namespace Web.Pages.Custom;

public partial class CategorySelector : ComponentBase
{
    private ElementReference inputElement;
    private string searchQuery = string.Empty;
    private List<CategoryExtendedDto> searchResults = new List<CategoryExtendedDto>();
    private int selectedIndex = -1;
    private bool showDropdown;

    [Parameter] public string LabelText { get; set; } = string.Empty;
    [Parameter] public bool AllowMultiple { get; set; } = true;
    [Parameter] public int CurrentCategoryId { get; set; }
    [Parameter] public List<CategoryExtendedDto>? SelectedCategories { get; set; }
    [Parameter] public CategoryExtendedDto? SelectedCategory { get; set; }
    [Parameter] public EventCallback<CategoryExtendedDto> OnCategoryChanged { get; set; }
    [Inject] private JavaScriptHandler? JSHandler { get; set; }
    [Inject] private ICategoryService? CategoryService { get; set; }

    private async Task SearchCategories(string? value)
    {
        searchQuery = value ?? string.Empty;
        selectedIndex = -1;

        if (CategoryService == null) return;

        searchResults = await CategoryService.GetFlatCategories(searchQuery);

        if (AllowMultiple)
        {
            if (SelectedCategories == null) return;

            searchResults = searchResults.Where(c => !SelectedCategories.Select(sc => sc.Id).Contains(c.Id)).ToList();
        }
        else
        {
            searchResults = searchResults.Where(c => c.Id != SelectedCategory?.Id && c.Id != CurrentCategoryId)
                .ToList();
        }
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

                if (JSHandler == null) return;

                await JSHandler.InvokeVoidAsync("scrollToActiveCategory", selectedIndex);
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

                if (JSHandler == null) return;

                await JSHandler.InvokeVoidAsync("scrollToActiveCategory", selectedIndex);
                break;
            }
            case "Enter" when selectedIndex >= 0:
                AddCategoryToSelection(searchResults[selectedIndex]);

                if (JSHandler == null) return;

                await JSHandler.InvokeVoidAsync("blurElement", inputElement);
                break;
        }
    }

    private async Task SelectCategory(int index)
    {
        selectedIndex = index;
        AddCategoryToSelection(searchResults[index]);

        if (JSHandler == null) return;

        await JSHandler.InvokeVoidAsync("blurElement", inputElement);
    }

    private void AddCategoryToSelection(CategoryExtendedDto category)
    {
        if (AllowMultiple)
        {
            if (SelectedCategories != null && !SelectedCategories.Contains(category))
            {
                SelectedCategories.Add(category);
            }
        }
        else
        {
            SelectedCategory = category;
            OnCategoryChanged.InvokeAsync(SelectedCategory);
        }

        searchResults.Clear();
        searchQuery = string.Empty;
        selectedIndex = -1;
    }

    private void RemoveCategory(CategoryExtendedDto category)
    {
        if (AllowMultiple)
        {
            if (SelectedCategories != null && SelectedCategories.Count != 0)
            {
                SelectedCategories.Remove(category);
            }
        }
        else
        {
            SelectedCategory = new CategoryExtendedDto();
            OnCategoryChanged.InvokeAsync(SelectedCategory);
        }
    }
}