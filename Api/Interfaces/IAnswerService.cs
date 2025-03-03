using QuestionaireApi.Models;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IAnswerService
{
    Task CreateQuestionAnswers(int questionId, ICollection<PendingAnswer> pendingAnswers);
    Task UpdateQuestionAnswers(int questionId, ICollection<Answer> answers, List<AnswerDto> updatedAnswers);
}