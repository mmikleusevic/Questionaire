using Web.Models;

namespace Web.Interfaces;

public interface IQuestionService
{
    Task<PaginatedResponse<Question>> GetQuestions(int currentPage, int pageSize);
    Task<Question> GetQuestion(int id);
    Task CreateQuestion(Question newQuestion);
    Task UpdateQuestion(Question updatedQuestion);
    Task DeleteQuestion(int id);
}