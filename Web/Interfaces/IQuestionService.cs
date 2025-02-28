using Web.Models;

namespace Web.Interfaces;

public interface IQuestionService
{
    Task<PaginatedResponse<Question>> GetQuestions(int currentPage, int pageSize);
    Task<Question> GetQuestion(int id);
    Task UpdateQuestion(Question updatedQuestion);
    Task DeleteQuestion(int id);
}