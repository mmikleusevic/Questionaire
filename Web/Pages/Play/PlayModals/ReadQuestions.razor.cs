using BlazorBootstrap;
using Microsoft.AspNetCore.Components;
using Shared.Models;
using SharedStandard.Models;
using Web.Interfaces;

namespace Web.Pages.Play.PlayModals;

public partial class ReadQuestions : ComponentBase
{
    private AnswerExtendedDto[] answers;
    private QuestionExtendedDto currentQuestion;
    private int currentQuestionIndex;
    private Button nextButton;
    private Button previousButton;
    [Inject] private IUserQuestionHistoryService UserQuestionHistoryService { get; set; }
    [Parameter] public Modal Modal { get; set; }
    [Parameter] public string deviceIdentifier { get; set; }
    [Parameter] public List<QuestionExtendedDto> Questions { get; set; }
    [Parameter] public bool IsSingleAnswerMode { get; set; }
    private bool IsPreviousButtonDisabled => currentQuestionIndex <= 0;
    private bool IsNextButtonDisabled => currentQuestionIndex >= Questions.Count - 1;
    private bool showAnswersOrStyling;

    protected override void OnParametersSet()
    {
        currentQuestionIndex = 0;
        showAnswersOrStyling = false;
        ShowQuestion(currentQuestionIndex);
    }

    private void ShowQuestion(int index)
    {
        if (index < 0 || index >= Questions.Count) return;

        currentQuestionIndex = index;
        currentQuestion = Questions[currentQuestionIndex];
        currentQuestion.isRead = true;

        answers = new AnswerExtendedDto[3];

        if (IsSingleAnswerMode)
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

    private async Task CreateUserQuestionHistory()
    {
        UserQuestionHistoryDto userQuestionHistoryDto = new UserQuestionHistoryDto
        {
            UserId = deviceIdentifier,
            QuestionIds = Questions.Where(q => q.isRead).Select(q => q.Id).ToList()
        };

        await UserQuestionHistoryService.CreateUserHistory(userQuestionHistoryDto);
    }

    private string GetDynamicAnswerClass(AnswerExtendedDto? answer)
    {
        if (answer == null) return "answer-item answer-default";

        string baseClass = "answer-item";
        string correctnessClass = answer.IsCorrect ? "answer-correct" : "answer-incorrect";

        if (IsSingleAnswerMode) return $"{baseClass} {correctnessClass}";

        return showAnswersOrStyling ? $"{baseClass} {correctnessClass}" : baseClass;
        
    }
    
    private async Task HideModal()
    {
        await CreateUserQuestionHistory();
        await Modal.HideAsync();
    }
}