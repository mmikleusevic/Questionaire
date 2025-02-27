using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Services;

public class PendingQuestionService(QuestionaireDbContext context) : IPendingQuestionService
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
            
            context.PendingQuestions.Add(dbQuestion);
            await context.SaveChangesAsync();
            
            List<PendingAnswer> dbPendingAnswers = pendingQuestion.PendingAnswers.Select(answer => new PendingAnswer
            {
                AnswerText = answer.AnswerText,
                IsCorrect = answer.IsCorrect,
                PendingQuestionId = dbQuestion.Id
            }).ToList();

            context.PendingAnswers.AddRange(dbPendingAnswers);
            
            List<PendingQuestionCategory> pendingQuestionCategories = pendingQuestion.Categories.Select(category => new PendingQuestionCategory
            {
                PendingQuestionId = dbQuestion.Id,
                CategoryId = category.Id
            }).ToList();

            context.PendingQuestionCategories.AddRange(pendingQuestionCategories);
            
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
            
            context.Questions.Add(newQuestion);
            await context.SaveChangesAsync();

            context.Answers.AddRange(pendingQuestion.PendingAnswers
                .Select(a => new Answer
                {
                    QuestionId = newQuestion.Id,
                    AnswerText = a.AnswerText,
                    IsCorrect = a.IsCorrect
                }));
            
            context.QuestionCategories.AddRange(pendingQuestion.PendingQuestionCategories
                .Select(pqc => new QuestionCategory
                {
                    QuestionId = newQuestion.Id,
                    CategoryId = pqc.CategoryId
                }));
            
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
    
    public async Task<bool> UpdatePendingQuestion(int id, UpdatePendingQuestionRequestDto updateRequest)
    {
        if (updateRequest.PendingAnswers.Count != 3 || 
            !updateRequest.PendingAnswers.Any(a => a.IsCorrect) || 
            updateRequest.CategoryIds.Count == 0)
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
