using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Models;
using Web.Interfaces;

namespace Web.Pages.Questions.QuestionModals;

public partial class UpdateQuestion : ComponentBase
{
    private EditContext? editContext;
    private List<CategoryValidationDto> selectedCategories = new List<CategoryValidationDto>();
    private QuestionValidationDto updatedQuestion = new QuestionValidationDto();

    private List<string> validationMessages = new List<string>();
    [Inject] private IQuestionService? QuestionService { get; set; }
    [Parameter] public QuestionValidationDto? Question { get; set; }
    [Parameter] public List<CategoryValidationDto>? FlatCategories { get; set; }
    [Parameter] public EventCallback OnQuestionChanged { get; set; }
    [Parameter] public Modal? Modal { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        List<AnswerValidationDto> existingAnswers = Question.Answers
            .Select(a => new AnswerValidationDto(a.Id)
            {
                AnswerText = a.AnswerText,
                IsCorrect = a.IsCorrect
            }).ToList();

        int additionalAnswersNeeded = Math.Max(0, 3 - existingAnswers.Count);

        List<AnswerValidationDto> newEmptyAnswers = Enumerable.Range(0, additionalAnswersNeeded)
            .Select(_ => new AnswerValidationDto
            {
                AnswerText = string.Empty,
                IsCorrect = false
            })
            .ToList();

        updatedQuestion = new QuestionValidationDto
        {
            QuestionText = Question.QuestionText,
            Answers = existingAnswers.Concat(newEmptyAnswers).ToList(),
            Categories = Question.Categories.Select(c => new CategoryValidationDto(c.Id)
            {
                CategoryName = c.CategoryName,
                ParentCategoryId = c.ParentCategoryId,
                ParentCategoryName = c.ParentCategoryName
            }).ToList()
        };

        selectedCategories = updatedQuestion.Categories.ToList();

        validationMessages.Clear();
        editContext = new EditContext(updatedQuestion);
    }

    public async Task HandleValidSubmit()
    {
        if (QuestionService == null) return;

        List<string> errorMessages = new List<string>();

        int correctAnswers = updatedQuestion.Answers.Count(a => a.IsCorrect);
        if (correctAnswers != 1)
        {
            errorMessages.Add("You have to mark exactly one answer as correct and 2 as incorrect!");
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

        updatedQuestion.Categories = selectedCategories.Where(a => a.Id != 0).ToList();

        Question.QuestionText = updatedQuestion.QuestionText;
        Question.Answers = updatedQuestion.Answers;
        Question.Categories = updatedQuestion.Categories;

        await QuestionService.UpdateQuestion(Question);
        await OnQuestionChanged.InvokeAsync();
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