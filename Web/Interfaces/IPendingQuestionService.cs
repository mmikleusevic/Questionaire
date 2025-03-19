using Shared.Models;

namespace Web.Interfaces;

public interface IPendingQuestionService
{
    Task<PaginatedResponse<PendingQuestionDto>> GetPendingQuestions(
        QuestionsRequestDto pendingQuestionsRequest);

    Task CreatePendingQuestion(PendingQuestionDto newPendingQuestion);
    Task ApprovePendingQuestion(int id);
    Task UpdatePendingQuestion(PendingQuestionDto updatedPendingQuestion);
    Task DeletePendingQuestion(int id);
}