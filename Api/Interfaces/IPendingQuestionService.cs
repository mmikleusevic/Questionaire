using System.Security.Claims;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IPendingQuestionService
{
    Task<PaginatedResponse<PendingQuestionDto>> GetPendingQuestions(QuestionsRequestDto pendingQuestionsRequestDto, ClaimsPrincipal user);
    Task CreatePendingQuestion(PendingQuestionDto pendingQuestion, ClaimsPrincipal user);
    Task<bool> ApprovePendingQuestion(int id, ClaimsPrincipal user);
    Task<bool> UpdatePendingQuestion(int id, PendingQuestionDto updatedPendingQuestion, ClaimsPrincipal user);
    Task<bool> DeletePendingQuestion(int id, ClaimsPrincipal user);
}