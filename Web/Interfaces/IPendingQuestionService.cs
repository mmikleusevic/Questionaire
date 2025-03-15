using Shared.Models;

namespace Web.Interfaces;

public interface IPendingQuestionService
{
    Task<PaginatedResponse<PendingQuestionValidationDto>> GetPendingQuestions(
        QuestionsRequestDto pendingQuestionsRequest);

    Task CreatePendingQuestion(PendingQuestionValidationDto newPendingQuestionValidation);
    Task ApprovePendingQuestion(int id);
    Task UpdatePendingQuestion(PendingQuestionValidationDto updatedPendingQuestionValidation);
    Task DeletePendingQuestion(int id);
}