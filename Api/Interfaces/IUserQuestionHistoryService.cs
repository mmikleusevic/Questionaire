using SharedStandard.Models;

namespace QuestionaireApi.Interfaces;

public interface IUserQuestionHistoryService
{
    Task ResetUserQuestionHistoryForCriteria(string userId, IEnumerable<int> categoryIds,
        IEnumerable<Difficulty> difficulties);

    Task CreateUserQuestionHistory(string userId, List<int> questionIds);
}