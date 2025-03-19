using Shared.Models;

namespace Web.Interfaces;

public interface IQuestionService
{
    Task<PaginatedResponse<QuestionExtendedDto>> GetQuestions(QuestionsRequestDto questionsRequest);
    Task UpdateQuestion(QuestionExtendedDto updatedQuestion);
    Task DeleteQuestion(int id);
}