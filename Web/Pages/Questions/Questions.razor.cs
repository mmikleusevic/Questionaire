using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using Web.Interfaces;
using Web.Pages.Questions.QuestionModals;
using UpdateQuestion = Web.Pages.Questions.QuestionModals.UpdateQuestion;

namespace Web.Pages.Questions;

public partial class Questions : ComponentBase
{
    private readonly QuestionsRequestDto questionsRequest = new QuestionsRequestDto
    {
        PageSize = 50,
        PageNumber = 1,
        OnlyMyQuestions = false
    };

    private List<CategoryValidationDto>? flatCategories;

    private Modal? modal;
    private List<QuestionValidationDto>? questions;
    private int totalPages = 1;
    [Inject] private IQuestionService? QuestionService { get; set; }
    [Inject] private ICategoryService? CategoryService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetQuestions();
        await GetFlatCategories();
    }

    private async Task GetQuestions()
    {
        if (QuestionService == null) return;

        PaginatedResponse<QuestionValidationDto> paginatedResponse =
            await QuestionService.GetQuestions(questionsRequest);
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
        questionsRequest.PageNumber = newPage;
        await GetQuestions();
        Navigation.NavigateTo(Navigation.Uri.Split('#')[0] + "#topElement", false);
    }

    private async Task ShowUpdateQuestion(QuestionValidationDto? question)
    {
        if (modal == null || question == null || flatCategories == null) return;

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "Question", question },
            { "FlatCategories", flatCategories },
            { "OnQuestionChanged", EventCallback.Factory.Create(this, GetQuestions) },
            { "Modal", modal }
        };

        await modal.ShowAsync<UpdateQuestion>("Update Question", parameters: parameters);
    }

    private async Task ShowDeleteQuestion(QuestionValidationDto? question)
    {
        if (modal == null || question == null) return;

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "Modal", modal },
            { "Question", question },
            { "OnQuestionChanged", EventCallback.Factory.Create(this, GetQuestions) }
        };

        await modal.ShowAsync<DeleteQuestion>("Delete Question", parameters: parameters);
    }

    private string GetAnswerRowClass(bool isCorrect)
    {
        return isCorrect ? "correct-answer" : "incorrect-answer";
    }

    private async Task ToggleOnlyMyQuestions(ChangeEventArgs e)
    {
        questionsRequest.OnlyMyQuestions = (bool)e.Value;
        questionsRequest.PageNumber = 1;
        await GetQuestions();
    }
    
    private async Task SearchQueryChanged(string value)
    {
        questionsRequest.SearchQuery = value;
        await GetQuestions();
    }
}