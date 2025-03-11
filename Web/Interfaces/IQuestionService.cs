using Web.Models;

namespace Web.Interfaces;

public interface IQuestionService
{
    Task<PaginatedResponse<Question>> GetQuestions(QuestionsRequest questionsRequest);
    Task UpdateQuestion(Question updatedQuestion);
    Task DeleteQuestion(int id);
}