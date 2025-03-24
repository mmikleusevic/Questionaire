using Microsoft.AspNetCore.Components;
using Shared.Models;
using Web.Pages.Custom;

namespace Web.Pages.Questions;

public partial class Questions : ComponentBase
{
    private readonly QuestionsRequestDto questionsRequest = new QuestionsRequestDto
    {
        PageSize = 50,
        PageNumber = 1,
        OnlyMyQuestions = false,
        FetchApprovedQuestions = true
    };

    private BaseQuestions? baseQuestionsRef;
}