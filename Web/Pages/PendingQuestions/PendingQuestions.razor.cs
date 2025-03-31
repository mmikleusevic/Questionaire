using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Shared.Models;
using Web.Pages.Custom;
using Web.Pages.Questions.QuestionModals;

namespace Web.Pages.PendingQuestions;

public partial class PendingQuestions : ComponentBase
{
    private readonly QuestionsRequestDto questionsRequest = new QuestionsRequestDto
    {
        PageSize = 50,
        PageNumber = 1,
        FetchApprovedQuestions = false
    };

    private BaseQuestions? baseQuestionsRef;
    [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }

    protected override async Task OnInitializedAsync()
    {
        AuthenticationState authState = await authenticationStateTask;
        ClaimsPrincipal currentUser = authState.User;

        if (currentUser.Identity?.IsAuthenticated == true)
        {
            if (currentUser.IsInRole("Admin") || currentUser.IsInRole("SuperAdmin"))
            {
                questionsRequest.OnlyMyQuestions = false;
            }
            else
            {
                questionsRequest.OnlyMyQuestions = true;
            }
        }
        else
        {
            questionsRequest.OnlyMyQuestions = false;
        }

        await base.OnInitializedAsync();
    }

    private async Task ShowApproveQuestion(QuestionExtendedDto? question)
    {
        if (baseQuestionsRef?.modal == null || question == null) return;

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "Modal", baseQuestionsRef?.modal },
            { "Question", question },
            { "OnQuestionChanged", baseQuestionsRef?.OnQuestionChanged }
        };

        await baseQuestionsRef?.modal.ShowAsync<ApproveQuestion>("Approve Question", parameters: parameters);
    }

    private async Task ShowCreateQuestion()
    {
        if (baseQuestionsRef?.modal == null) return;

        Dictionary<string, object> parameters = new Dictionary<string, object>
        {
            { "OnQuestionChanged", baseQuestionsRef?.OnQuestionChanged },
            { "Modal", baseQuestionsRef?.modal }
        };

        await baseQuestionsRef?.modal.ShowAsync<CreateQuestion>("Create New Question", parameters: parameters);
    }
}