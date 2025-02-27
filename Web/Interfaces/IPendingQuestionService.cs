using Web.Models;

namespace Web.Interfaces;

public interface IPendingQuestionService
{
    Task<PaginatedResponse<PendingQuestion>> GetPendingQuestions(int currentPage, int pageSize);
    Task<PendingQuestion> GetPendingQuestion(int id);
    Task CreatePendingQuestion(PendingQuestion newPendingQuestion);
    Task ApprovePendingQuestion(int id);
    Task UpdatePendingQuestion(PendingQuestion updatedPendingQuestion);
    Task DeletePendingQuestion(int id);
}