using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;

namespace QuestionaireApi.Services;

public class AnswerService(QuestionaireDbContext context) : IAnswerService
{
    public async Task<List<Answer>> GetAnswersAsync()
    {
        return await context.Answers.ToListAsync();
    }

    public async Task<Answer?> GetAnswerByIdAsync(int id)
    {
        return await context.Answers.FindAsync(id);
    }

    public async Task<Answer> AddAnswerAsync(Answer answer)
    {
        context.Answers.Add(answer);
        await context.SaveChangesAsync();
        return answer;
    }

    public async Task<bool> UpdateAnswerAsync(int id, Answer updatedAnswer)
    {
        Answer answer = await context.Answers.FindAsync(id);
        if (answer == null) return false;

        answer.AnswerText = updatedAnswer.AnswerText;
        answer.IsCorrect = updatedAnswer.IsCorrect;
        
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAnswerAsync(int id)
    {
        Answer answer = await context.Answers.FindAsync(id);
        if (answer == null) return false;

        context.Answers.Remove(answer);
        await context.SaveChangesAsync();
        return true;
    }
}