using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IUserQuestionHistoryService
{
    Task ResetUserQuestionHistory(string userId);
    Task CreateUserQuestionHistory(string userId, List<Question> questions);
}