using QuestionaireApi.Models.Database;

namespace QuestionaireApi.Interfaces;

public interface IUserQuestionHistoryService
{
    Task ResetUserQuestionHistory(string userId);
    Task CreateUserQuestionHistory(string userId, List<Question> questions);
}