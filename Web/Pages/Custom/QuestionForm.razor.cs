using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Shared.Models;

namespace Web.Pages.Custom;

public partial class QuestionForm : ComponentBase
{
    private EditContext editContext;

    private List<CategoryExtendedDto> selectedCategories = new List<CategoryExtendedDto>();
    private QuestionExtendedDto updatedQuestion = new QuestionExtendedDto();
    private List<string> validationMessages = new List<string>();
    [Parameter] public QuestionExtendedDto Question { get; set; }
    [Parameter] public EventCallback OnSubmit { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
    [Parameter] public string TitleText { get; set; }
    [Parameter] public string SubmitButtonText { get; set; } = "Submit";

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        List<AnswerExtendedDto> existingAnswers = Question.Answers
            .Select(a => new AnswerExtendedDto(a.Id)
            {
                AnswerText = a.AnswerText,
                IsCorrect = a.IsCorrect
            }).ToList();

        int additionalAnswersNeeded = Math.Max(0, 3 - existingAnswers.Count);

        List<AnswerExtendedDto> newEmptyAnswers = Enumerable.Range(0, additionalAnswersNeeded)
            .Select(_ => new AnswerExtendedDto
            {
                AnswerText = string.Empty,
                IsCorrect = false
            })
            .ToList();

        updatedQuestion = new QuestionExtendedDto
        {
            QuestionText = Question.QuestionText,
            Answers = existingAnswers.Concat(newEmptyAnswers).ToList(),
            Categories = Question.Categories.Select(c => new CategoryExtendedDto(c.Id)
            {
                CategoryName = c.CategoryName,
                ParentCategoryId = c.ParentCategoryId
            }).ToList()
        };

        selectedCategories = updatedQuestion.Categories.ToList();

        validationMessages.Clear();
        editContext = new EditContext(updatedQuestion);
    }

    private async Task HandleValidSubmit()
    {
        List<string> errorMessages = new List<string>();

        int correctAnswers = updatedQuestion.Answers.Count(a => a.IsCorrect);
        if (correctAnswers != 1)
        {
            errorMessages.Add("You have to mark exactly one answer as correct and 2 as incorrect!");
        }

        if (selectedCategories == null || !selectedCategories.Any())
        {
            errorMessages.Add("You have to add at least one category!");
        }

        if (errorMessages.Any())
        {
            validationMessages = errorMessages;
            return;
        }

        Question.Categories = selectedCategories.Where(a => a.Id != 0).ToList();
        Question.Answers = updatedQuestion.Answers;
        Question.QuestionText = updatedQuestion.QuestionText;

        await OnSubmit.InvokeAsync();
    }
}