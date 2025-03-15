using QuestionaireApi.Models.Database;
using Shared.Models;

namespace QuestionaireApi.Interfaces;

public interface IPendingAnswerService
{
    Task CreatePendingQuestionAnswers(int pendingQuestionId, List<PendingAnswerValidationDto> pendingAnswers);

    Task UpdatePendingQuestionAnswers(int pendingQuestionId, ICollection<PendingAnswer> pendingAnswers,
        List<PendingAnswerValidationDto> updatedPendingAnswers);
}