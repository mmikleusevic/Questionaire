using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using SharedStandard.Models;

namespace QuestionaireApi.Services;

public class UserQuestionHistoryService(QuestionaireDbContext context) : IUserQuestionHistoryService
{
    public async Task ResetUserQuestionHistoryForCriteria(string userId, IEnumerable<int> categoryIds, IEnumerable<Difficulty> difficulties)
    {
        try
        {
            List<int> questionIdsMatchingCriteria = await context.Questions
                .Where(q => q.IsApproved == true && q.IsDeleted == false)
                .Where(q => difficulties.Contains(q.Difficulty))
                .Where(q => q.QuestionCategories.Any(qc => categoryIds.Contains(qc.CategoryId)))
                .Select(q => q.Id)
                .Distinct()
                .ToListAsync();

            if (!questionIdsMatchingCriteria.Any()) return;
            
            await context.UserQuestionHistory
                .Where(h => h.UserId == userId && questionIdsMatchingCriteria.Contains(h.QuestionId))
                .ExecuteDeleteAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"An error occurred while resetting question history for user with ID {userId}.", ex);
        }
    }

    public async Task CreateUserQuestionHistory(string userId, List<int> questionIds)
    {
        try
        {
            await context.UserQuestionHistory.AddRangeAsync(questionIds.Select(q => new UserQuestionHistory
            {
                UserId = userId,
                QuestionId = q,
            }));
            
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"An error occurred while creating question history for user with ID {userId}.", ex);
        }
    }
}