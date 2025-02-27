using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IPendingQuestionService
{
    Task<PaginatedResponse<PendingQuestionDto>> GetPendingQuestions(int pageNumber, int pageSize);
    Task<PendingQuestion?> GetPendingQuestion(int id);
    Task ApproveQuestion(int id);
    Task CreatePendingQuestion(PendingQuestionDto pendingQuestion);
    Task<bool> DeletePendingQuestion(int id);
    Task<bool> UpdatePendingQuestion(int id, UpdatePendingQuestionRequestDto updateRequest);

}