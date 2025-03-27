using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using Web.Interfaces;

namespace Web.Pages.Questions.QuestionModals;

public partial class CreateQuestion : ComponentBase
{
    private QuestionExtendedDto question;

    [Inject] private IQuestionService? QuestionService { get; set; }
    [Parameter] public EventCallback OnQuestionChanged { get; set; }
    [Parameter] public Modal? Modal { get; set; }

    protected override void OnParametersSet()
    {
        question = new QuestionExtendedDto();
    }

    public async Task HandleValidSubmit()
    {
        await QuestionService.CreateQuestion(question);
        await OnQuestionChanged.InvokeAsync();
        await Hide();
    }

    private async Task Hide()
    {
        if (Modal == null) return;

        await Modal.HideAsync();
    }
}