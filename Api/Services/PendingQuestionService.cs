using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Services;

public class PendingQuestionService(QuestionaireDbContext context) : IPendingQuestionService
{
    public async Task<List<PendingQuestion>> GetPendingQuestionsAsync()
    {
        try
        {
            return await context.PendingQuestions
                .Include(q => q.Answers)
                .Include(q => q.PendingQuestionCategories)
                .ThenInclude(q => q.Category)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while retrieving pending questions.", ex);
        }
    }
    
    public async Task<PendingQuestion?> GetPendingQuestionAsync(int pendingQuestionId)
    {
        try
        {
            return await context.PendingQuestions
                .Include(q => q.Answers)
                .Include(q => q.PendingQuestionCategories)
                .ThenInclude(q => q.Category)
                .FirstOrDefaultAsync(q => q.Id == pendingQuestionId);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while retrieving the pending question with ID {pendingQuestionId}.", ex);
        }
    }
    
    public async Task ApproveQuestion(int pendingQuestionId)
    {
        try
        {
            PendingQuestion? pendingQuestion = await context.PendingQuestions
                .Include(q => q.Answers)
                .Include(q => q.PendingQuestionCategories)
                .FirstOrDefaultAsync(q => q.Id == pendingQuestionId);

            if (pendingQuestion == null) throw new InvalidOperationException("Pending question not found.");
            
            Question newQuestion = new Question
            {
                QuestionText = pendingQuestion.QuestionText
            };
            
            context.Questions.Add(newQuestion);
            await context.SaveChangesAsync();
            
            foreach (PendingAnswer pendingAnswer in pendingQuestion.Answers)
            {
                Answer answer = new Answer
                {
                    QuestionId = newQuestion.Id,
                    AnswerText = pendingAnswer.AnswerText,
                    IsCorrect = pendingAnswer.IsCorrect
                };
                
                context.Answers.Add(answer);
            }
            
            foreach (PendingQuestionCategory pendingQuestionCategory in pendingQuestion.PendingQuestionCategories)
            {
                QuestionCategory questionCategory = new QuestionCategory
                {
                    QuestionId = newQuestion.Id,
                    CategoryId = pendingQuestionCategory.CategoryId
                };
                
                context.QuestionCategories.Add(questionCategory);
            }
            
            context.PendingQuestions.Remove(pendingQuestion);

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while approving the question.", ex);
        }
    }
    
    public async Task CreatePendingQuestion(PendingQuestion pendingQuestion)
    {
        try
        {
            if (pendingQuestion.Answers.Count != 3 || 
                !pendingQuestion.Answers.Any(a => a.IsCorrect) || 
                pendingQuestion.PendingQuestionCategories.Count == 0)
            {
                throw new InvalidOperationException("Invalid question: must have exactly 3 answers, 1 correct answer and at least one category.");
            }
            
            context.PendingQuestions.Add(pendingQuestion);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while creating the pending question.", ex);
        }
    }
    
    public async Task<bool> UpdatePendingQuestion(int pendingQuestionId, UpdatePendingQuestionRequestDto updateRequest)
    {
        try
        {
            PendingQuestion? pendingQuestion = await context.PendingQuestions
                .Include(q => q.Answers)
                .Include(q => q.PendingQuestionCategories)
                .FirstOrDefaultAsync(q => q.Id == pendingQuestionId);

            if (pendingQuestion == null) return false;
            
            if (updateRequest.Answers.Count != 3 || !updateRequest.Answers.Any(a => a.IsCorrect) || updateRequest.CategoryIds.Count == 0)
            {
                throw new InvalidOperationException("Invalid question: must have exactly 3 answers, 1 correct answer and at least one category.");
            }

            pendingQuestion.QuestionText = updateRequest.QuestionText;
            
            context.PendingAnswers.RemoveRange(pendingQuestion.Answers);
            context.PendingQuestionCategories.RemoveRange(pendingQuestion.PendingQuestionCategories);
            
            foreach (PendingAnswer newAnswer in updateRequest.Answers)
            {
                PendingAnswer answer = new PendingAnswer
                {
                    PendingQuestionId = pendingQuestion.Id,
                    AnswerText = newAnswer.AnswerText,
                    IsCorrect = newAnswer.IsCorrect
                };
                context.PendingAnswers.Add(answer);
            }
            
            foreach (int categoryId in updateRequest.CategoryIds)
            {
                PendingQuestionCategory pendingQuestionCategory = new PendingQuestionCategory
                {
                    PendingQuestionId = pendingQuestion.Id,
                    CategoryId = categoryId
                };
                
                context.PendingQuestionCategories.Add(pendingQuestionCategory);
            }
            
            await context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while updating the pending question with ID {pendingQuestionId}.", ex);
        }
    }
    
    public async Task<bool> DeletePendingQuestion(int pendingQuestionId)
    {
        try
        {
            PendingQuestion? pendingQuestion = await context.PendingQuestions
                .Include(q => q.Answers)
                .Include(q => q.PendingQuestionCategories)
                .FirstOrDefaultAsync(q => q.Id == pendingQuestionId);
            
            if (pendingQuestion == null) return false;
            
            context.PendingAnswers.RemoveRange(pendingQuestion.Answers);
            context.PendingQuestionCategories.RemoveRange(pendingQuestion.PendingQuestionCategories);
            context.PendingQuestions.Remove(pendingQuestion);
            
            await context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while deleting the pending question.", ex);
        }
    }
}
