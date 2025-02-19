using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;

namespace QuestionaireApi.Services;

public class AnswerService(QuestionaireDbContext context) : IAnswerService
{
    public async Task<List<Answer>> GetAnswersAsync()
    {
        try
        {
            return await context.Answers.ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving answers.", ex);
        }
    }

    public async Task<Answer?> GetAnswerByIdAsync(int id)
    {
        try
        {
            return await context.Answers.FindAsync(id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving the answer by ID.", ex);
        }
    }

    public async Task CreateAnswerAsync(Answer answer)
    {
        try
        {
            context.Answers.Add(answer);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while creating the answer.", ex);
        }
    }

    public async Task<bool> UpdateAnswerAsync(int id, Answer updatedAnswer)
    {
        try
        {
            Answer? answer = await context.Answers.FindAsync(id);
            if (answer == null) return false;
            
            answer.AnswerText = updatedAnswer.AnswerText;
            answer.IsCorrect = updatedAnswer.IsCorrect;

            await context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while updating the answer.", ex);
        }
    }

    public async Task<bool> DeleteAnswerAsync(int id)
    {
        try
        {
            Answer? answer = await context.Answers.FirstOrDefaultAsync(a => a.Id == id);
            if (answer == null) return false;

            context.Answers.Remove(answer);
            await context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while deleting the answer.", ex);
        }
    }
}