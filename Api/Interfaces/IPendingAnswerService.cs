using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IPendingAnswerService
{
    Task CreatePendingQuestionAnswers(int pendingQuestionId, List<PendingAnswerDto> pendingAnswers);
    Task UpdatePendingQuestionAnswers(int pendingQuestionId, ICollection<PendingAnswer> pendingAnswers, List<PendingAnswerDto> updatedPendingAnswers);
}