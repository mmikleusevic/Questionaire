using Microsoft.AspNetCore.Components;
using Web.Interfaces;
using Web.Models;

namespace Web.Components.Pages.Questions;

public partial class Questions : ComponentBase
{
    [Inject] private IQuestionService? QuestionService { get; set; }
    
    private List<Question>? questions;

    protected override async Task OnInitializedAsync()
    {
        await GetQuestions();
    }

    private async Task GetQuestions()
    {
        if (QuestionService == null) return;
        
        questions = await QuestionService.GetQuestions();
    }
    
    private void ShowCreateQuestion()
    {
        Console.WriteLine($"Create a question");
    }

    private string GetAnswerRowClass(bool isCorrect)
    {
        return isCorrect ? "correct-answer" : "incorrect-answer";
    }

    private void ShowUpdateQuestion(int id)
    {
        Console.WriteLine($"Updating question with ID: {id}");
    }

    private void ShowDeleteQuestion(int id)
    {
        Console.WriteLine($"Deleting question with ID: {id}");
    }
}