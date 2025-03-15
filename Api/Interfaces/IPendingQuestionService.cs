using System.Security.Claims;
using Shared.Models;

namespace QuestionaireApi.Interfaces;

public interface IPendingQuestionService
{
    Task<PaginatedResponse<PendingQuestionValidationDto>> GetPendingQuestions(
        QuestionsRequestDto pendingQuestionsRequestDto,
        ClaimsPrincipal user);

    Task CreatePendingQuestion(PendingQuestionValidationDto pendingQuestion, ClaimsPrincipal user);
    Task<bool> ApprovePendingQuestion(int id, ClaimsPrincipal user);
    Task<bool> UpdatePendingQuestion(int id, PendingQuestionValidationDto updatedPendingQuestion, ClaimsPrincipal user);
    Task<bool> DeletePendingQuestion(int id, ClaimsPrincipal user);
}