using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Web.Interfaces;
using Web.Models;

namespace Web.Pages.PendingQuestions.PendingQuestionModal;

public partial class CreatePendingQuestion : ComponentBase
{
    private readonly PendingQuestion pendingQuestion = new PendingQuestion();
    private readonly List<Category> selectedCategories = new List<Category>();
    private EditContext? editContext;

    private List<string> validationMessages = new List<string>();
    [Inject] private IPendingQuestionService? PendingQuestionService { get; set; }
    [Parameter] public EventCallback OnPendingQuestionChanged { get; set; }
    [Parameter] public List<Category> FlatCategories { get; set; }
    [Parameter] public Modal? Modal { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        SetAnswers();
    }

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        pendingQuestion.QuestionText = string.Empty;

        selectedCategories.Clear();
        selectedCategories.Add(new Category());

        if (pendingQuestion?.PendingAnswers == null || pendingQuestion.PendingAnswers.Count == 0)
        {
            SetAnswers();
        }
        else
        {
            foreach (PendingAnswer pendingAnswer in pendingQuestion.PendingAnswers)
            {
                pendingAnswer.AnswerText = string.Empty;
                pendingAnswer.IsCorrect = false;
            }
        }

        validationMessages.Clear();

        editContext = new EditContext(pendingQuestion);
    }

    public async Task HandleValidSubmit()
    {
        if (PendingQuestionService == null) return;

        List<string> errorMessages = new List<string>();

        int correctAnswers = pendingQuestion.PendingAnswers.Count(a => a.IsCorrect);
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

        pendingQuestion.Categories = selectedCategories.Where(a => a.Id != 0).ToList();
        await PendingQuestionService.CreatePendingQuestion(pendingQuestion);
        await OnPendingQuestionChanged.InvokeAsync();
        await Hide();
    }

    private void SetAnswers()
    {
        for (int i = 0; i < 3; i++)
        {
            pendingQuestion?.PendingAnswers?.Add(new PendingAnswer { AnswerText = string.Empty });
        }
    }

    private void AddCategoryDropdown()
    {
        selectedCategories.Add(new Category());
    }

    private void RemoveCategoryDropdown()
    {
        if (selectedCategories.Count > 1)
        {
            selectedCategories.RemoveAt(selectedCategories.Count - 1);
        }
    }

    private void SelectCategory(Category currentCategory, Category newCategory)
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