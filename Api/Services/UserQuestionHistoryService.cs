using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Services;

public class UserQuestionHistoryService(QuestionaireDbContext context) : IUserQuestionHistoryService
{
    public async Task ResetUserQuestionHistory(string userId)
    {
        try
        {
            await context.UserQuestionHistory
                .Where(h => h.UserId == userId)
                .ExecuteDeleteAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while resetting question history for user with ID {userId}.", ex);
        }
    }

    public async Task CreateUserQuestionHistory(string userId, List<Question> questions)
    {
        try
        {
            await context.UserQuestionHistory.AddRangeAsync(questions.Select(q => new UserQuestionHistory
            {
                UserId = userId,
                QuestionId = q.Id,
                RoundNumber = 1
            }));

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while creating question history for user with ID {userId}.", ex);
        }
    }
}