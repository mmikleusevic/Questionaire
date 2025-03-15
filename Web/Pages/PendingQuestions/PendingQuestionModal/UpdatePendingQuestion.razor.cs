using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Models;
using Web.Interfaces;

namespace Web.Pages.PendingQuestions.PendingQuestionModal;

public partial class UpdatePendingQuestion : ComponentBase
{
    private EditContext? editContext;
    private List<CategoryValidationDto> selectedCategories = new List<CategoryValidationDto>();
    private PendingQuestionValidationDto updatedPendingQuestionValidation = new PendingQuestionValidationDto();

    private List<string> validationMessages = new List<string>();
    [Inject] private IPendingQuestionService? PendingQuestionService { get; set; }
    [Parameter] public PendingQuestionValidationDto? PendingQuestion { get; set; }
    [Parameter] public List<CategoryValidationDto>? FlatCategories { get; set; }
    [Parameter] public EventCallback OnPendingQuestionChanged { get; set; }
    [Parameter] public Modal? Modal { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        List<PendingAnswerValidationDto> existingPendingAnswers = PendingQuestion.PendingAnswers
            .Select(a => new PendingAnswerValidationDto(a.Id)
            {
                AnswerText = a.AnswerText,
                IsCorrect = a.IsCorrect
            }).ToList();

        int additionalPendingAnswersNeeded = Math.Max(0, 3 - existingPendingAnswers.Count);

        List<PendingAnswerValidationDto> newEmptyPendingAnswers = Enumerable.Range(0, additionalPendingAnswersNeeded)
            .Select(_ => new PendingAnswerValidationDto
            {
                AnswerText = string.Empty,
                IsCorrect = false
            })
            .ToList();

        updatedPendingQuestionValidation = new PendingQuestionValidationDto
        {
            QuestionText = PendingQuestion.QuestionText,
            PendingAnswers = existingPendingAnswers.Concat(newEmptyPendingAnswers).ToList(),
            Categories = PendingQuestion.Categories.Select(c => new CategoryValidationDto(c.Id)
            {
                CategoryName = c.CategoryName,
                ParentCategoryId = c.ParentCategoryId
            }).ToList()
        };

        selectedCategories = updatedPendingQuestionValidation.Categories.ToList();

        validationMessages.Clear();
        editContext = new EditContext(updatedPendingQuestionValidation);
    }

    public async Task HandleValidSubmit()
    {
        if (PendingQuestionService == null) return;

        List<string> errorMessages = new List<string>();

        int correctAnswers = updatedPendingQuestionValidation.PendingAnswers.Count(a => a.IsCorrect);
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

        updatedPendingQuestionValidation.Categories = selectedCategories.Where(a => a.Id != 0).ToList();

        PendingQuestion.QuestionText = updatedPendingQuestionValidation.QuestionText;
        PendingQuestion.PendingAnswers = updatedPendingQuestionValidation.PendingAnswers;
        PendingQuestion.Categories = updatedPendingQuestionValidation.Categories;

        await PendingQuestionService.UpdatePendingQuestion(PendingQuestion);
        await OnPendingQuestionChanged.InvokeAsync();
        await Hide();
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