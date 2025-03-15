using Shared.Models;

namespace Web.Interfaces;

public interface IQuestionService
{
    Task<PaginatedResponse<QuestionValidationDto>> GetQuestions(QuestionsRequestDto questionsRequest);
    Task UpdateQuestion(QuestionValidationDto updatedQuestion);
    Task DeleteQuestion(int id);
}