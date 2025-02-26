using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Web.Interfaces;
using Web.Models;

namespace Web.Components.Pages.Questions.QuestionModals;

public partial class UpdateQuestion : ComponentBase
{
    [Inject] private IQuestionService? QuestionService { get; set; }
    [Parameter] public Question? Question { get; set; }
    [Parameter] public List<Category>? FlatCategories { get; set; }
    [Parameter] public EventCallback OnQuestionChanged { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    
    private List<string> validationMessages = new List<string>();
    private EditContext? editContext;
    private Question updatedQuestion = new Question();
    private List<Category> selectedCategories = new List<Category>();

    protected override async Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();

        updatedQuestion = new Question
        {
            QuestionText = Question.QuestionText,
            Answers = Question.Answers.Select(a => new Answer
            {
                AnswerText = a.AnswerText,
                IsCorrect = a.IsCorrect
            })
            .Concat(Enumerable.Repeat(new Answer
                {
                    AnswerText = string.Empty, 
                    IsCorrect = false
                }, 3 - Question.Answers.Count)
                .Take(Math.Max(0, 3 - Question.Answers.Count)
                )
            )
            .ToList(),
            Categories = Question.Categories.Select(c => new Category(
                c.Id,
                c.CategoryName,
                c.ParentCategoryId
            )).ToList()
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