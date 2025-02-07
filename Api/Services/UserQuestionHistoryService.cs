using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Services;

public class UserQuestionHistoryService(QuestionaireDbContext context) : IUserQuestionHistoryService
{
    public async Task ResetUserQuestionHistory(string userId)
    {
        await context.UserQuestionHistory
            .Where(h => h.UserId == userId)
            .ExecuteDeleteAsync();
        
        await context.SaveChangesAsync();
    }

    public async Task SaveUserQuestionHistory(string userId, List<QuestionDto> questions)
    {
        List<UserQuestionHistory> newHistory = questions.Select(q => new UserQuestionHistory
        {
            UserId = userId,
            QuestionId = q.Id,
            RoundNumber = 1
        }).ToList();
        
        await context.UserQuestionHistory.AddRangeAsync(newHistory);
        await context.SaveChangesAsync();
    }
}