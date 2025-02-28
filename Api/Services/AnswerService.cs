using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Services;

public class AnswerService(QuestionaireDbContext context) : IAnswerService
{
    public async Task<List<Answer>> GetAnswers()
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

    public async Task<Answer?> GetAnswerById(int id)
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

    public async Task CreateAnswer(AnswerDto answer)
    {
        try
        {
            
            
            context.Answers.Add(new Answer
            {
                AnswerText = answer.AnswerText,
                IsCorrect = answer.IsCorrect,
            });
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while creating the answer.", ex);
        }
    }

    public async Task<bool> UpdateAnswer(int id, AnswerDto updatedAnswer)
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

    public async Task<bool> DeleteAnswer(int id)
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