using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Web.Interfaces;
using Web.Models;

namespace Web.Components.Pages.Questions.QuestionModals;

public partial class DeleteQuestion : ComponentBase
{
    [Inject] IQuestionService? QuestionService { get; set; }
    [Parameter] public Modal? Modal { get; set; }
    [Parameter] public Question? Question { get; set; }
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