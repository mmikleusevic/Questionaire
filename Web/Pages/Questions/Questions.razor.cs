using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Interfaces;
using Web.Models;
using Web.Pages.Questions.QuestionModals;
using UpdateQuestion = Web.Pages.Questions.QuestionModals.UpdateQuestion;

namespace Web.Pages.Questions;

public partial class Questions : ComponentBase
{
    [Inject] private IQuestionService? QuestionService { get; set; }
    [Inject] private ICategoryService? CategoryService { get; set; }
    
    private Modal? modal;
    private List<Question>? questions;
    private List<Category>? flatCategories;
    private const int PageSize = 50;
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
    
    private async Task ShowUpdateQuestion(Question? question)
    {
        if (modal == null || question == null || flatCategories == null) return;
        
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "Question", question },
            { "FlatCategories", flatCategories },
            { "OnQuestionChanged", EventCallback.Factory.Create(this, GetQuestions) },
            { "Modal", modal },
        };
        
        await modal.ShowAsync<UpdateQuestion>("Update Question",  parameters: parameters);
    }
    
    private async Task ShowDeleteQuestion(Question? question)
    {
        if (modal == null || question == null) return;
        
        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "Modal", modal },
            { "Question", question },
            { "OnQuestionChanged", EventCallback.Factory.Create(this, GetQuestions)}
        };
        
        await modal.ShowAsync<DeleteQuestion>("Delete Question",  parameters: parameters);
    }

    private string GetAnswerRowClass(bool isCorrect) => isCorrect ? "correct-answer" : "incorrect-answer";
}