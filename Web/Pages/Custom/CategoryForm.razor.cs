using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Models;

namespace Web.Pages.Custom;

public partial class CategoryForm : ComponentBase
{
    private EditContext? editContext;
    private CategoryExtendedDto? selectedParentCategory;
    [Parameter] public CategoryExtendedDto Category { get; set; } = new();
    [Parameter] public List<CategoryExtendedDto>? FlatCategories { get; set; }
    [Parameter] public EventCallback<CategoryExtendedDto> OnSubmit { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public string SubmitButtonText { get; set; } = "Submit";

    protected override void OnInitialized()
    {
        editContext = new EditContext(Category);
        selectedParentCategory = FlatCategories?.FirstOrDefault(c => c.Id == Category.ParentCategoryId);
    }

    private async Task HandleValidSubmit()
    {
        Category.ParentCategoryId = selectedParentCategory?.Id == 0 ? null : selectedParentCategory?.Id;
        await OnSubmit.InvokeAsync(Category);
    }

    private async Task Hide()
    {
        await OnClose.InvokeAsync();
    }
}