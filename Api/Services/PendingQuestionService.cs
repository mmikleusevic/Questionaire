using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Services;

public class PendingQuestionService(QuestionaireDbContext context,
    IPendingAnswerService pendingAnswerService,
    IPendingQuestionCategoriesService pendingQuestionCategoriesService,
    IAnswerService answerService,
    IQuestionCategoriesService questionCategoriesService) : IPendingQuestionService
{
    public async Task<PaginatedResponse<PendingQuestionDto>> GetPendingQuestions(int pageNumber, int pageSize)
    {
        try
        {
            List<PendingQuestion> questions = await context.PendingQuestions
                .Include(a => a.PendingAnswers)
                .Include(a => a.PendingQuestionCategories)
                .ThenInclude(c=> c.Category)
                .OrderBy(q => q.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            int totalQuestions = await context.PendingQuestions.CountAsync();

            PaginatedResponse<PendingQuestionDto> response = new PaginatedResponse<PendingQuestionDto>
            {
                Items = questions.Select(q => new PendingQuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    PendingAnswers = q.PendingAnswers.Select(a => new PendingAnswerDto
                    {
                        Id = a.Id,
                        AnswerText = a.AnswerText,
                        IsCorrect = a.IsCorrect
                    }).ToList(),
                    Categories = q.PendingQuestionCategories.Select(qc => new CategoryDto
                    {
                        Id = qc.Category.Id,
                        CategoryName = qc.Category.CategoryName
                    }).ToList()
                }).ToList(),
                TotalCount = totalQuestions,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalQuestions / pageSize)
            };
            
            return response;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving pending questions.", ex);
        }
    }
    
    public async Task CreatePendingQuestion(PendingQuestionDto pendingQuestion)
    {
        if (pendingQuestion.PendingAnswers.Count != 3 || 
            !pendingQuestion.PendingAnswers.Any(a => a.IsCorrect) || 
            pendingQuestion.Categories.Count == 0)
        {
            throw new InvalidOperationException("Invalid pending question: must have exactly 3 pending answers, 1 correct pending answer and at least one category.");
        }
        
        await using IDbContextTransaction? transaction = await context.Database.BeginTransactionAsync();
        
        try
        {
            PendingQuestion dbQuestion = new PendingQuestion
            {
                QuestionText = pendingQuestion.QuestionText
            };
            
            await context.PendingQuestions.AddAsync(dbQuestion);
            await context.SaveChangesAsync();
            
            await pendingAnswerService.CreatePendingQuestionAnswers(dbQuestion.Id, pendingQuestion.PendingAnswers);
            await pendingQuestionCategoriesService.CreatePendingQuestionCategories(dbQuestion.Id,
                pendingQuestion.Categories);
            
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("An error occurred while creating the pending question.", ex);
        }
    }
    
    public async Task<bool> ApprovePendingQuestion(int id)
    {
        await using IDbContextTransaction? transaction = await context.Database.BeginTransactionAsync();
        
        try
        {
            PendingQuestion? pendingQuestion = await context.PendingQuestions
                .Include(q => q.PendingAnswers)
                .Include(q => q.PendingQuestionCategories)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (pendingQuestion == null)
            {
                await transaction.RollbackAsync();
                return false;
            }
            
            if (pendingQuestion.PendingAnswers.Count != 3 || 
                !pendingQuestion.PendingAnswers.Any(a => a.IsCorrect) || 
                pendingQuestion.PendingQuestionCategories.Count == 0)
            {
                throw new InvalidOperationException("Invalid pending question: must have exactly 3 pending answers, 1 correct pending answer and at least one category.");
            }
            
            Question newQuestion = new Question
            {
                QuestionText = pendingQuestion.QuestionText
            };
            
            await context.Questions.AddAsync(newQuestion);
            await context.SaveChangesAsync();

            await answerService.CreateQuestionAnswers(newQuestion.Id, pendingQuestion.PendingAnswers);
            await questionCategoriesService.CreateQuestionCategories(newQuestion.Id, pendingQuestion.PendingQuestionCategories);
            
            context.PendingQuestions.Remove(pendingQuestion);
            
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("An error occurred while approving the pending question.", ex);
        }
    }
    
    public async Task<bool> UpdatePendingQuestion(int id, PendingQuestionDto updatedPendingQuestion)
    {
        if (updatedPendingQuestion.PendingAnswers.Count != 3 || 
            !updatedPendingQuestion.PendingAnswers.Any(a => a.IsCorrect) || 
            updatedPendingQuestion.Categories.Count == 0)
        {
            throw new InvalidOperationException("Invalid pending question: must have exactly 3 pending answers, 1 correct pending answer and at least one category.");
        }
        
        await using IDbContextTransaction? transaction = await context.Database.BeginTransactionAsync();
        
        try
        {
            PendingQuestion? pendingQuestion = await context.PendingQuestions
                .Include(q => q.PendingAnswers)
                .Include(q => q.PendingQuestionCategories)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (pendingQuestion == null)
            {
                await transaction.RollbackAsync();
                return false;
            }

            pendingQuestion.QuestionText = updatedPendingQuestion.QuestionText;
            
            await pendingAnswerService.UpdatePendingQuestionAnswers(pendingQuestion.Id, pendingQuestion.PendingAnswers, updatedPendingQuestion.PendingAnswers);
            await pendingQuestionCategoriesService.UpdatePendingQuestionCategories(pendingQuestion.Id, pendingQuestion.PendingQuestionCategories, updatedPendingQuestion.Categories);
            
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException($"An error occurred while updating the pending question with ID {id}.", ex);
        }
    }
    
    public async Task<bool> DeletePendingQuestion(int id)
    {
        try
        {
            PendingQuestion? pendingQuestion = await context.PendingQuestions
                .Include(a => a.PendingAnswers)
                .FirstOrDefaultAsync(a => a.Id == id);
            
            if (pendingQuestion == null) return false;
            
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
