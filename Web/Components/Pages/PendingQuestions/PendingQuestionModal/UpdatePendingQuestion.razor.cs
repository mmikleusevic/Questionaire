using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Web.Interfaces;
using Web.Models;

namespace Web.Components.Pages.PendingQuestions.PendingQuestionModal;

public partial class UpdatePendingQuestion : ComponentBase
{
    [Inject] private IPendingQuestionService? PendingQuestionService { get; set; }
    [Parameter] public PendingQuestion? PendingQuestion { get; set; }
    [Parameter] public List<Category>? FlatCategories { get; set; }
    [Parameter] public EventCallback OnPendingQuestionChanged { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    
    private List<string> validationMessages = new List<string>();
    private EditContext? editContext;
    private PendingQuestion updatedPendingQuestion = new PendingQuestion();
    private List<Category> selectedCategories = new List<Category>();

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();
        
        List<PendingAnswer> existingPendingAnswers = PendingQuestion.PendingAnswers
            .Select(a => new PendingAnswer
            (
                a.Id,
                a.AnswerText,
                a.IsCorrect
            ))
            .ToList();
        
        int additionalPendingAnswersNeeded = Math.Max(0, 3 - existingPendingAnswers.Count);
        
        List<PendingAnswer> newEmptyPendingAnswers = Enumerable.Range(0, additionalPendingAnswersNeeded)
            .Select(_ => new PendingAnswer
            {
                AnswerText = string.Empty,
                IsCorrect = false
            })
            .ToList();
        
        updatedPendingQuestion = new PendingQuestion
        {
            QuestionText = PendingQuestion.QuestionText,
            PendingAnswers = existingPendingAnswers.Concat(newEmptyPendingAnswers).ToList(),
            Categories = PendingQuestion.Categories.Select(c => new Category(
                c.Id,
                c.CategoryName,
                c.ParentCategoryId
            )).ToList()
        };

        selectedCategories = updatedPendingQuestion.Categories.ToList();
    
        validationMessages.Clear();
        editContext = new EditContext(updatedPendingQuestion);
    }
    
    public async Task HandleValidSubmit()
    {
        if (PendingQuestionService == null) return;
        
        List<string> errorMessages = new List<string>();
        
        int correctAnswers = updatedPendingQuestion.PendingAnswers.Count(a => a.IsCorrect);
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
        
        updatedPendingQuestion.Categories = selectedCategories.Where(a => a.Id != 0).ToList();
        
        PendingQuestion.QuestionText = updatedPendingQuestion.QuestionText;
        PendingQuestion.PendingAnswers = updatedPendingQuestion.PendingAnswers;
        PendingQuestion.Categories = updatedPendingQuestion.Categories;
        
        await PendingQuestionService.UpdatePendingQuestion(PendingQuestion);
        await OnPendingQuestionChanged.InvokeAsync();
        await Hide();
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