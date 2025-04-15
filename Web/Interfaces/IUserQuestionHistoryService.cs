using SharedStandard.Models;

namespace Web.Interfaces;

public interface IUserQuestionHistoryService
{
    Task CreateUserHistory(UserQuestionHistoryDto userQuestionHistoryDto);
}