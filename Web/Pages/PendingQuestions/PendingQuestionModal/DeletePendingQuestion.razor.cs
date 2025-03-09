using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Interfaces;
using Web.Models;

namespace Web.Pages.PendingQuestions.PendingQuestionModal;

public partial class DeletePendingQuestion : ComponentBase
{
    [Inject] private IPendingQuestionService? PendingQuestionService { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public PendingQuestion? PendingQuestion { get; set; }
    [Parameter] public EventCallback OnPendingQuestionChanged { get; set; }

    private async Task HandleValidSubmit()
    {
        if (PendingQuestion == null || PendingQuestionService == null) return;

        await PendingQuestionService.DeletePendingQuestion(PendingQuestion.Id);
        await OnPendingQuestionChanged.InvokeAsync(PendingQuestion);
        await Hide();
    }

    private async Task Hide()
    {
        if (Modal == null) return;

        await Modal.HideAsync();
    }
}