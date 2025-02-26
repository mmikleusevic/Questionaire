using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Services;

public class PendingQuestionService(QuestionaireDbContext context) : IPendingQuestionService
{
    public async Task<List<PendingQuestion>> GetPendingQuestions()
    {
        try
        {
            return await context.PendingQuestions
                .Include(q => q.PendingAnswers)
                .Include(q => q.PendingQuestionCategories)
                .ThenInclude(q => q.Category)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving pending questions.", ex);
        }
    }
    
    public async Task<PendingQuestion?> GetPendingQuestion(int id)
    {
        try
        {
            return await context.PendingQuestions
                .Include(q => q.PendingAnswers)
                .Include(q => q.PendingQuestionCategories)
                .ThenInclude(q => q.Category)
                .FirstOrDefaultAsync(q => q.Id == id);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while retrieving the pending question with ID {id}.", ex);
        }
    }
    
    public async Task ApproveQuestion(int id)
    {
        try
        {
            PendingQuestion? pendingQuestion = await context.PendingQuestions
                .Include(q => q.PendingAnswers)
                .Include(q => q.PendingQuestionCategories)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (pendingQuestion == null) throw new InvalidOperationException("Pending question not found.");
            
            Question newQuestion = new Question
            {
                QuestionText = pendingQuestion.QuestionText
            };
            
            context.Questions.Add(newQuestion);
            await context.SaveChangesAsync();

            context.PendingAnswers.AddRange(pendingQuestion.PendingAnswers
                .Select(a => new PendingAnswer
                {
                    PendingQuestionId = newQuestion.Id,
                    AnswerText = a.AnswerText,
                    IsCorrect = a.IsCorrect
                }));
            
            context.PendingQuestionCategories.AddRange(pendingQuestion.PendingQuestionCategories
                .Select(pqc => new PendingQuestionCategory
                {
                    PendingQuestionId = newQuestion.Id,
                    CategoryId = pqc.CategoryId
                }));
            
            context.PendingQuestions.Remove(pendingQuestion);

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while approving the question.", ex);
        }
    }
    
    public async Task CreatePendingQuestion(PendingQuestion updatedPendingQuestion)
    {
        try
        {
            if (updatedPendingQuestion.PendingAnswers.Count != 3 || 
                !updatedPendingQuestion.PendingAnswers.Any(a => a.IsCorrect) || 
                updatedPendingQuestion.PendingQuestionCategories.Count == 0)
            {
                throw new InvalidOperationException("Invalid question: must have exactly 3 answers, 1 correct answer and at least one category.");
            }
            
            context.PendingQuestions.Add(updatedPendingQuestion);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while creating the pending question.", ex);
        }
    }
    
    public async Task<bool> UpdatePendingQuestion(int id, UpdatePendingQuestionRequestDto updateRequest)
    {
        try
        {
            PendingQuestion? pendingQuestion = await context.PendingQuestions
                .Include(q => q.PendingAnswers)
                .Include(q => q.PendingQuestionCategories)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (pendingQuestion == null) return false;
            
            if (updateRequest.PendingAnswers.Count != 3 || !updateRequest.PendingAnswers.Any(a => a.IsCorrect) || updateRequest.CategoryIds.Count == 0)
            {
                throw new InvalidOperationException("Invalid question: must have exactly 3 answers, 1 correct answer and at least one category.");
            }

            pendingQuestion.QuestionText = updateRequest.QuestionText;
            
            pendingQuestion.PendingAnswers = updateRequest.PendingAnswers.Select(a => new PendingAnswer
            {
                PendingQuestionId = pendingQuestion.Id,
                AnswerText = a.AnswerText,
                IsCorrect = a.IsCorrect
            }).ToList();

            pendingQuestion.PendingQuestionCategories = updateRequest.CategoryIds.Select(c => new PendingQuestionCategory
            {
                PendingQuestionId = pendingQuestion.Id,
                CategoryId = c
            }).ToList();
            
            await context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while updating the pending question with ID {id}.", ex);
        }
    }
    
    public async Task<bool> DeletePendingQuestion(int id)
    {
        try
        {
            PendingQuestion? pendingQuestion = await context.PendingQuestions
                .Include(q => q.PendingAnswers)
                .Include(q => q.PendingQuestionCategories)
                .FirstOrDefaultAsync(q => q.Id == id);
            
            if (pendingQuestion == null) return false;
            
            context.PendingAnswers.RemoveRange(pendingQuestion.PendingAnswers);
            context.PendingQuestionCategories.RemoveRange(pendingQuestion.PendingQuestionCategories);
            context.PendingQuestions.Remove(pendingQuestion);
            
            await context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while deleting the pending question.", ex);
        }
    }
}
