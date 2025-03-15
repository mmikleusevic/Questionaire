using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using Web.Interfaces;
using Web.Pages.PendingQuestions.PendingQuestionModal;
using CreatePendingQuestion = Web.Pages.PendingQuestions.PendingQuestionModal.CreatePendingQuestion;
using DeletePendingQuestion = Web.Pages.PendingQuestions.PendingQuestionModal.DeletePendingQuestion;
using UpdatePendingQuestion = Web.Pages.PendingQuestions.PendingQuestionModal.UpdatePendingQuestion;

namespace Web.Pages.PendingQuestions;

public partial class PendingQuestions : ComponentBase
{
    private readonly QuestionsRequestDto pendingQuestionsRequest = new QuestionsRequestDto
    {
        PageSize = 50,
        PageNumber = 1,
        OnlyMyQuestions = false
    };

    private List<CategoryValidationDto>? flatCategories;

    private Modal? modal;
    private List<PendingQuestionValidationDto>? pendingQuestions;
    private int totalPages = 1;
    [Inject] private IPendingQuestionService? PendingQuestionService { get; set; }
    [Inject] private ICategoryService? CategoryService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await GetPendingQuestions();
        await GetFlatCategories();
    }

    private async Task GetPendingQuestions()
    {
        if (PendingQuestionService == null) return;

        PaginatedResponse<PendingQuestionValidationDto> paginatedResponse =
            await PendingQuestionService.GetPendingQuestions(pendingQuestionsRequest);
        pendingQuestions = paginatedResponse.Items;
        totalPages = paginatedResponse.TotalPages;
    }

    private async Task GetFlatCategories()
    {
        if (CategoryService == null) return;

        flatCategories = await CategoryService.GetFlatCategories();
    }

    private async Task OnPageChanged(int newPage)
    {
        pendingQuestionsRequest.PageNumber = newPage;
        await GetPendingQuestions();
        Navigation.NavigateTo(Navigation.Uri.Split('#')[0] + "#topElement", false);
    }

    private async Task ShowCreatePendingQuestion()
    {
        if (modal == null || flatCategories == null) return;

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "OnPendingQuestionChanged", EventCallback.Factory.Create(this, GetPendingQuestions) },
            { "FlatCategories", flatCategories },
            { "Modal", modal }
        };

        await modal.ShowAsync<CreatePendingQuestion>("Create New Pending Question", parameters: parameters);
    }

    private async Task ShowApprovePendingQuestion(PendingQuestionValidationDto? pendingQuestion)
    {
        if (modal == null || pendingQuestion == null) return;

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "Modal", modal },
            { "PendingQuestion", pendingQuestion },
            { "OnPendingQuestionChanged", EventCallback.Factory.Create(this, GetPendingQuestions) }
        };

        await modal.ShowAsync<ApprovePendingQuestion>("Approve Pending Question", parameters: parameters);
    }

    private async Task ShowUpdatePendingQuestion(PendingQuestionValidationDto? pendingQuestion)
    {
        if (modal == null || pendingQuestion == null || flatCategories == null) return;

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "PendingQuestion", pendingQuestion },
            { "FlatCategories", flatCategories },
            { "OnPendingQuestionChanged", EventCallback.Factory.Create(this, GetPendingQuestions) },
            { "Modal", modal }
        };

        await modal.ShowAsync<UpdatePendingQuestion>("Update Pending Question", parameters: parameters);
    }

    private async Task ShowDeletePendingQuestion(PendingQuestionValidationDto? pendingQuestion)
    {
        if (modal == null || pendingQuestion == null) return;

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "Modal", modal },
            { "PendingQuestion", pendingQuestion },
            { "OnPendingQuestionChanged", EventCallback.Factory.Create(this, GetPendingQuestions) }
        };

        await modal.ShowAsync<DeletePendingQuestion>("Delete Pending Question", parameters: parameters);
    }

    private string GetAnswerRowClass(bool isCorrect)
    {
        return isCorrect ? "correct-pending-answer" : "incorrect-pending-answer";
    }

    private async Task ToggleOnlyMyPendingQuestions(ChangeEventArgs e)
    {
        pendingQuestionsRequest.OnlyMyQuestions = (bool)e.Value;
        pendingQuestionsRequest.PageNumber = 1;
        await GetPendingQuestions();
    }
}