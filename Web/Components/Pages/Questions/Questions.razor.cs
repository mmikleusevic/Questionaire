using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Components.Pages.Questions.QuestionModals;
using Web.Interfaces;
using Web.Models;

namespace Web.Components.Pages.Questions;

public partial class Questions : ComponentBase
{
    private const int PageSize = 50;
    [Inject] private IQuestionService? QuestionService { get; set; }
    [Inject] private ICategoryService? CategoryService { get; set; }
    
    private Modal modal = null!;
    private List<Question>? questions;
    private List<Category>? flatCategories;
    private int currentPage = 1;
    private int totalPages = 1;

    protected override async Task OnInitializedAsync()
    {
        await GetQuestions();
        await GetFlatCategories();
    }

    private async Task GetQuestions()
    {
        if (QuestionService == null) return;
        
        PaginatedResponse<Question> paginatedResponse = await QuestionService.GetQuestions(currentPage, PageSize);
        questions = paginatedResponse.Items;
        totalPages = paginatedResponse.TotalPages;
    }
    
    private async Task GetFlatCategories()
    {
        if (CategoryService == null) return;
        
        flatCategories = await CategoryService.GetFlatCategories();
    }

    private async Task OnPageChanged(int newPage)
    {
        currentPage = newPage;
        await GetQuestions();
        Navigation.NavigateTo(Navigation.Uri.Split('#')[0] + "#topElement", forceLoad: false);
    }

    private async Task ShowCreateQuestion()
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "OnQuestionCreated", EventCallback.Factory.Create(this, GetQuestions) },
            { "FlatCategories", flatCategories },
            { "Modal", modal }
        };
        
        await modal.ShowAsync<CreateQuestion>(title: "Create New Question", parameters: parameters);
    }

    private string GetAnswerRowClass(bool isCorrect) => isCorrect ? "correct-answer" : "incorrect-answer";

    private void ShowUpdateQuestion(int id)
    {
        Console.WriteLine($"Updating question with ID: {id}");
    }

    private void ShowDeleteQuestion(int id)
    {
        Console.WriteLine($"Deleting question with ID: {id}");
    }
}