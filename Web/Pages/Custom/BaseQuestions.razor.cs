using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using Web.Interfaces;
using Web.Pages.Questions.QuestionModals;

namespace Web.Pages.Custom;

public partial class BaseQuestions : ComponentBase
{
    private bool isInitialized = false;
    public Modal? modal;

    private List<QuestionExtendedDto>? questions;
    private bool shouldFetch = false;
    private int totalPages = 1;
    public EventCallback OnQuestionChanged => EventCallback.Factory.Create(this, GetQuestions);

    [Parameter] public string Title { get; set; }
    [Parameter] public string EmptyText { get; set; }
    [Parameter] public string ToggleRoles { get; set; }
    [Parameter] public RenderFragment? AdditionalTopControls { get; set; }
    [Parameter] public RenderFragment<QuestionExtendedDto>? QuestionActions { get; set; }
    [Parameter] public QuestionsRequestDto? QuestionsRequest { get; set; }
    [Inject] private IQuestionService? QuestionService { get; set; }
    [Inject] private NavigationManager Navigation { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetQuestions();
    }

    private async Task GetQuestions()
    {
        if (QuestionsRequest == null || QuestionService == null) return;

        PaginatedResponse<QuestionExtendedDto> paginatedResponse = await QuestionService.GetQuestions(QuestionsRequest);
        questions = paginatedResponse.Items;
        totalPages = paginatedResponse.TotalPages;
    }

    private async Task OnPageChanged(int newPage)
    {
        if (QuestionsRequest == null) return;

        QuestionsRequest.PageNumber = newPage;
        await GetQuestions();
        Navigation.NavigateTo(Navigation.Uri.Split('#')[0] + "#topElement");
    }

    public async Task ShowUpdateQuestion(QuestionExtendedDto? question)
    {
        if (modal == null || question == null) return;

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "Question", question },
            { "OnQuestionChanged", EventCallback.Factory.Create(this, GetQuestions) },
            { "Modal", modal }
        };

        await modal.ShowAsync<UpdateQuestion>("Update Question", parameters: parameters);
    }

    public async Task ShowDeleteQuestion(QuestionExtendedDto? question)
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
        if (QuestionsRequest == null) return;

        QuestionsRequest.OnlyMyQuestions = (bool)e.Value;
        QuestionsRequest.PageNumber = 1;
        await GetQuestions();
    }

    private async Task SearchQueryChanged(string value)
    {
        if (QuestionsRequest == null) return;

        QuestionsRequest.SearchQuery = value;
        QuestionsRequest.PageNumber = 1;
        await GetQuestions();
    }
}