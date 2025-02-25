using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IPendingQuestionService
{
    Task<List<PendingQuestion>> GetPendingQuestions();
    Task<PendingQuestion?> GetPendingQuestion(int id);
    Task ApproveQuestion(int id);
    Task CreatePendingQuestion(PendingQuestion updatedPendingQuestion);
    Task<bool> DeletePendingQuestion(int id);
    Task<bool> UpdatePendingQuestion(int id, UpdatePendingQuestionRequestDto updateRequest);

}