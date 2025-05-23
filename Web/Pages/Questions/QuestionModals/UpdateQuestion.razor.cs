using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using Web.Interfaces;

namespace Web.Pages.Questions.QuestionModals;

public partial class UpdateQuestion : ComponentBase
{
    [Inject] private IQuestionService? QuestionService { get; set; }
    [Parameter] public QuestionExtendedDto? Question { get; set; }
    [Parameter] public EventCallback OnQuestionChanged { get; set; }
    [Parameter] public Modal? Modal { get; set; }

    public async Task HandleValidSubmit()
    {
        if (QuestionService == null) return;

        await QuestionService.UpdateQuestion(Question);
        await OnQuestionChanged.InvokeAsync();
        await Hide();
    }

    private async Task Hide()
    {
        if (Modal == null) return;

        await Modal.HideAsync();
    }
}