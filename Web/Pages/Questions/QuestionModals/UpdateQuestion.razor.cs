using System.Runtime.InteropServices;
using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Shared.Models;
using Web.Interfaces;

namespace Web.Pages.Questions.QuestionModals;

public partial class UpdateQuestion : ComponentBase
{
    private EditContext? editContext;
    private List<CategoryExtendedDto> selectedCategories = new List<CategoryExtendedDto>();
    private QuestionExtendedDto updatedQuestion = new QuestionExtendedDto();
    private List<CategoryExtendedDto> searchResults = new List<CategoryExtendedDto>();
    private string searchQuery = string.Empty;
    private int selectedIndex = -1;

    private List<string> validationMessages = new List<string>();
    [Inject] private IQuestionService? QuestionService { get; set; }
    [Inject] private ICategoryService? CategoryService { get; set; }
    [Parameter] public QuestionExtendedDto? Question { get; set; }
    [Parameter] public List<CategoryExtendedDto>? FlatCategories { get; set; }
    [Parameter] public EventCallback OnQuestionChanged { get; set; }
    [Parameter] public Modal? Modal { get; set; }

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
        searchQuery = string.Empty;
        selectedIndex = -1;
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

    private async Task SearchCategories(string? value)
    {
        searchQuery = value ?? string.Empty;
        selectedIndex = -1;
        
        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            if (CategoryService == null) return;
            
            searchResults = await CategoryService.GetFlatCategories(searchQuery);

            if (selectedCategories.Count == 0) return;
            
            searchResults = searchResults.Where(c => !selectedCategories.Select(sc => sc.Id).Contains(c.Id)).ToList();
        }
        else
        {
            searchResults.Clear();
        }
    }
    
    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "ArrowDown")
        {
            selectedIndex = (selectedIndex + 1) % searchResults.Count;
        }
        else if (e.Key == "ArrowUp")
        {
            selectedIndex = (selectedIndex - 1 + searchResults.Count) % searchResults.Count;
        }
        else if (e.Key == "Enter" && selectedIndex >= 0)
        {
            AddCategoryToSelection(searchResults[selectedIndex]);
        }
    }
    
    private void SelectCategory(int index)
    {
        selectedIndex = index;
        AddCategoryToSelection(searchResults[index]);
    }
    
    private void AddCategoryToSelection(CategoryExtendedDto category)
    {
        if (!selectedCategories.Contains(category))
        {
            selectedCategories.Add(category);
        }
        
        searchResults.Clear();
        searchQuery = string.Empty;
        selectedIndex = -1;
    }

    private void RemoveCategory()
    {
        if (selectedCategories.Count > 0)
        {
            selectedCategories.RemoveAt(selectedCategories.Count - 1);
        }
    }

    private async Task Hide()
    {
        if (Modal == null) return;

        await Modal.HideAsync();
    }
}