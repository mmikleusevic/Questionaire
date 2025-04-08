using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Shared.Models;

namespace Web.Pages.Play.PlayModals;

public partial class ReadQuestions : ComponentBase
{
    private AnswerExtendedDto[] answers;
    private QuestionExtendedDto currentQuestion;
    private int currentQuestionIndex;
    private Button nextButton;
    private Button previousButton;
    [Parameter] public Modal Modal { get; set; }
    [Parameter] public List<QuestionExtendedDto> Questions { get; set; }
    [Parameter] public bool IsSingleAnswerMode { get; set; }
    private bool IsPreviousButtonDisabled => currentQuestionIndex <= 0;
    private bool IsNextButtonDisabled => currentQuestionIndex >= Questions.Count - 1;

    protected override void OnParametersSet()
    {
        currentQuestionIndex = 0;
        ShowQuestion(currentQuestionIndex);
    }

    private void ShowQuestion(int index)
    {
        if (index < 0 || index >= Questions.Count) return;

        currentQuestionIndex = index;
        currentQuestion = Questions[currentQuestionIndex];
        currentQuestion.isRead = true;

        answers = new AnswerExtendedDto[3];

        bool hasOneAnswer = currentQuestion.Answers.Count == 1;

        if (IsSingleAnswerMode || hasOneAnswer)
        {
            AnswerExtendedDto correctAnswer = currentQuestion?.Answers?.FirstOrDefault(a => a.IsCorrect);

            if (correctAnswer == null) return;

            answers[1] = correctAnswer;
        }
        else
        {
            for (int i = 0; i < currentQuestion.Answers.Count; i++)
            {
                answers[i] = currentQuestion.Answers[i];
            }
        }
    }

    private void PreviousQuestion()
    {
        if (IsPreviousButtonDisabled) return;
        ShowQuestion(currentQuestionIndex - 1);
    }

    private void NextQuestion()
    {
        if (IsNextButtonDisabled) return;
        ShowQuestion(currentQuestionIndex + 1);
    }

    private string GetAnswerClass(AnswerExtendedDto? answer)
    {
        if (answer == null) return "answer-default";

        return answer.IsCorrect ? "answer-correct" : "answer-incorrect";
    }

    private async Task HideModal()
    {
        await Modal.HideAsync();
    }
}