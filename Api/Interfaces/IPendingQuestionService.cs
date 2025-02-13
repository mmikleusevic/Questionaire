using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IPendingQuestionService
{
    Task<List<PendingQuestion>> GetPendingQuestionsAsync();
    Task<PendingQuestion?> GetPendingQuestionAsync(int pendingQuestionId);
    Task ApproveQuestion(int pendingQuestionId);
    Task CreatePendingQuestion(PendingQuestion pendingQuestion);
    Task<bool> DeletePendingQuestion(int pendingQuestionId);
    Task<bool> UpdatePendingQuestion(int pendingQuestionId, UpdatePendingQuestionRequestDto updateRequest);

}