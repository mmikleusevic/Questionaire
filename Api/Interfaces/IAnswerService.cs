using QuestionaireApi.Models.Database;
using SharedStandard.Models;

namespace QuestionaireApi.Interfaces;

public interface IAnswerService
{
    Task CreateQuestionAnswers(int questionId, ICollection<PendingAnswer> pendingAnswers);
    Task UpdateQuestionAnswers(int questionId, ICollection<Answer> answers, List<AnswerDto> updatedAnswers);
}