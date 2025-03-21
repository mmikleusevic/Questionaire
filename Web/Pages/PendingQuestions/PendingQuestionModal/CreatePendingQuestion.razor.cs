using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Shared.Models;
using Web.Interfaces;

namespace Web.Pages.PendingQuestions.PendingQuestionModal;

public partial class CreatePendingQuestion : ComponentBase
{
    private readonly PendingQuestionDto pendingQuestion = new PendingQuestionDto();
    private readonly List<CategoryExtendedDto> selectedCategories = new List<CategoryExtendedDto>();
    private EditContext? editContext;
    private List<CategoryExtendedDto> searchResults = new List<CategoryExtendedDto>();
    private string searchQuery = string.Empty;
    private int selectedIndex = -1;

    private List<string> validationMessages = new List<string>();
    [Inject] private IPendingQuestionService? PendingQuestionService { get; set; }
    [Inject] private ICategoryService? CategoryService { get; set; }
    [Parameter] public EventCallback OnPendingQuestionChanged { get; set; }
    [Parameter] public List<CategoryExtendedDto> FlatCategories { get; set; }
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

        if (pendingQuestion?.PendingAnswers == null || pendingQuestion.PendingAnswers.Count == 0)
        {
            SetAnswers();
        }
        else
        {
            foreach (PendingAnswerDto pendingAnswer in pendingQuestion.PendingAnswers)
            {
                pendingAnswer.AnswerText = string.Empty;
                pendingAnswer.IsCorrect = false;
            }
        }

        validationMessages.Clear();
        searchQuery = string.Empty;
        selectedIndex = -1;
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
            pendingQuestion?.PendingAnswers?.Add(new PendingAnswerDto
                { AnswerText = string.Empty });
        }
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