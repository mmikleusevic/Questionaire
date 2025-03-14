using Shared.Models;

namespace Web.Interfaces;

public interface IQuestionService
{
    Task<PaginatedResponse<QuestionDto>> GetQuestions(QuestionsRequestDto questionsRequest);
    Task UpdateQuestion(QuestionDto updatedQuestion);
    Task DeleteQuestion(int id);
}