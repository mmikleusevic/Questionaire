using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Models;

namespace Web.Pages.Custom;

public partial class CategoryForm : ComponentBase
{
    private EditContext? editContext;
    private CategoryExtendedDto? selectedParentCategory;
    private CategoryExtendedDto updatedCategory = new CategoryExtendedDto();
    [Parameter] public CategoryExtendedDto Category { get; set; } = new CategoryExtendedDto();
    [Parameter] public List<CategoryExtendedDto>? FlatCategories { get; set; }
    [Parameter] public EventCallback<CategoryExtendedDto> OnSubmit { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }
    [Parameter] public string SubmitButtonText { get; set; } = "Submit";

    protected override void OnParametersSet()
    {
        updatedCategory.ParentCategoryId = Category.ParentCategoryId;
        updatedCategory.CategoryName = Category.CategoryName;

        editContext = new EditContext(updatedCategory);
        selectedParentCategory = FlatCategories?.FirstOrDefault(c => c.Id == updatedCategory.ParentCategoryId);
    }

    private async Task HandleValidSubmit()
    {
        Category.ParentCategoryId = updatedCategory.ParentCategoryId;
        Category.CategoryName = updatedCategory.CategoryName;

        await OnSubmit.InvokeAsync(Category);
    }

    private void OnCategoryChanged(CategoryExtendedDto? category)
    {
        selectedParentCategory = category;
        updatedCategory.ParentCategoryId = selectedParentCategory?.Id == 0 ? null : selectedParentCategory?.Id;
    }

    private async Task Hide()
    {
        await OnClose.InvokeAsync();
    }
}