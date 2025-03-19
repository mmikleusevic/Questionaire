using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using Web.Interfaces;

namespace Web.Pages.Questions.QuestionModals;

public partial class DeleteQuestion : ComponentBase
{
    [Inject] private IQuestionService? QuestionService { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public QuestionExtendedDto? Question { get; set; }
    [Parameter] public EventCallback OnQuestionChanged { get; set; }

    private async Task HandleValidSubmit()
    {
        if (Question == null || QuestionService == null) return;

        await QuestionService.DeleteQuestion(Question.Id);
        await OnQuestionChanged.InvokeAsync(Question);
        await Hide();
    }

    private async Task Hide()
    {
        if (Modal == null) return;

        await Modal.HideAsync();
    }
}