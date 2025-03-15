using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Models;
using Web.Interfaces;

namespace Web.Pages.PendingQuestions.PendingQuestionModal;

public partial class CreatePendingQuestion : ComponentBase
{
    private readonly PendingQuestionValidationDto pendingQuestionValidation = new PendingQuestionValidationDto();
    private readonly List<CategoryValidationDto> selectedCategories = new List<CategoryValidationDto>();
    private EditContext? editContext;

    private List<string> validationMessages = new List<string>();
    [Inject] private IPendingQuestionService? PendingQuestionService { get; set; }
    [Parameter] public EventCallback OnPendingQuestionChanged { get; set; }
    [Parameter] public List<CategoryValidationDto> FlatCategories { get; set; }
    [Parameter] public Modal? Modal { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        SetAnswers();
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        pendingQuestionValidation.QuestionText = string.Empty;

        selectedCategories.Clear();
        selectedCategories.Add(new CategoryValidationDto());

        if (pendingQuestionValidation?.PendingAnswers == null || pendingQuestionValidation.PendingAnswers.Count == 0)
        {
            SetAnswers();
        }
        else
        {
            foreach (PendingAnswerValidationDto pendingAnswer in pendingQuestionValidation.PendingAnswers)
            {
                pendingAnswer.AnswerText = string.Empty;
                pendingAnswer.IsCorrect = false;
            }
        }

        validationMessages.Clear();

        editContext = new EditContext(pendingQuestionValidation);
    }

    public async Task HandleValidSubmit()
    {
        if (PendingQuestionService == null) return;

        List<string> errorMessages = new List<string>();

        int correctAnswers = pendingQuestionValidation.PendingAnswers.Count(a => a.IsCorrect);
        if (correctAnswers != 1)
        {
            errorMessages.Add("You have to mark exactly one pending answer as correct and 2 as incorrect!");
        }

        int numberOfCategories = selectedCategories.Select(a => a.Id).Count(a => a != 0);
        if (numberOfCategories == 0)
        {
            errorMessages.Add("You have to add at least one category!");
        }

        if (errorMessages.Any())
        {
            validationMessages = errorMessages;
            return;
        }

        pendingQuestionValidation.Categories = selectedCategories.Where(a => a.Id != 0).ToList();
        await PendingQuestionService.CreatePendingQuestion(pendingQuestionValidation);
        await OnPendingQuestionChanged.InvokeAsync();
        await Hide();
    }

    private void SetAnswers()
    {
        for (int i = 0; i < 3; i++)
        {
            pendingQuestionValidation?.PendingAnswers?.Add(new PendingAnswerValidationDto
                { AnswerText = string.Empty });
        }
    }

    private void AddCategoryDropdown()
    {
        selectedCategories.Add(new CategoryValidationDto());
    }

    private void RemoveCategoryDropdown()
    {
        if (selectedCategories.Count > 1)
        {
            selectedCategories.RemoveAt(selectedCategories.Count - 1);
        }
    }

    private void SelectCategory(CategoryValidationDto currentCategory, CategoryValidationDto newCategory)
    {
        int categoryIndex = selectedCategories.IndexOf(currentCategory);

        selectedCategories[categoryIndex] = newCategory;
    }

    private async Task Hide()
    {
        if (Modal == null) return;

        await Modal.HideAsync();
    }
}