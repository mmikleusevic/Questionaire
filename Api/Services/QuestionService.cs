using System.Diagnostics;
using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Services;

public class QuestionService(QuestionaireDbContext context, 
    IUserQuestionHistoryService userQuestionHistoryService,
    IAnswerService answerService,
    IQuestionCategoriesService questionCategoriesService,
    IUserService userService) : IQuestionService
{
    public async Task<PaginatedResponse<QuestionDto>> GetQuestions(int pageNumber, int pageSize)
    {
        try
        {
            List<Question> questions = await context.Questions
                .Include(a => a.Answers)
                .Include(a => a.QuestionCategories)
                    .ThenInclude(c=> c.Category)
                .OrderBy(q => q.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            
            int totalQuestions = await context.Questions.CountAsync();

            PaginatedResponse<QuestionDto> response = new PaginatedResponse<QuestionDto>
            {
                Items = questions.Select(q => new QuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Answers = q.Answers.Select(a => new AnswerDto
                    {
                        Id = a.Id,
                        AnswerText = a.AnswerText,
                        IsCorrect = a.IsCorrect
                    }).ToList(),
                    Categories = q.QuestionCategories.Select(qc => new CategoryDto
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
            throw new InvalidOperationException("An error occurred while retrieving the list of questions.", ex);
        }
    }
    
    public async Task<List<QuestionDto>> GetRandomUniqueQuestions(GetRandomUniqueQuestionsRequestDto requestDto)
    {
        try
        {
            IQueryable<Question> baseQuery = context.Questions
                .Include(q => q.Answers)
                .Where(q => q.Answers.Count >= (requestDto.IsSingleAnswerMode ? 1 : 3))
                .Where(q => q.Answers.Any(a => a.IsCorrect))
                .Where(q => q.QuestionCategories.Any(qc => requestDto.CategoryIds.Contains(qc.CategoryId)))
                .Where(q => !context.UserQuestionHistory.Any(h => h.UserId == requestDto.UserId && h.QuestionId == q.Id));
        
            List<Question> questions = await FetchRandomQuestions(baseQuery, requestDto.NumberOfQuestions, null);
        
            if (questions.Count < requestDto.NumberOfQuestions)
            {
                await userQuestionHistoryService.ResetUserQuestionHistory(requestDto.UserId);

                int remainingQuestionsCount = requestDto.NumberOfQuestions - questions.Count;
                HashSet<int> idsToExclude = questions.Select(q => q.Id).ToHashSet();
                List<Question> additionalQuestions = await FetchRandomQuestions(baseQuery, remainingQuestionsCount, idsToExclude);

                questions.AddRange(additionalQuestions);
            }
        
            await userQuestionHistoryService.CreateUserQuestionHistory(requestDto.UserId, questions);
        
            return MapQuestionsToDtos(questions, requestDto.IsSingleAnswerMode);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while retrieving the random questions.", ex);
        }
    }

    public async Task<bool> UpdateQuestion(int id, QuestionDto updatedQuestion, ClaimsPrincipal user)
    {
        await using IDbContextTransaction? transaction = await context.Database.BeginTransactionAsync();
        
        try
        {
            string userId = await userService.GetUserId(user);
        
            if (updatedQuestion.Answers.Count != 3 || 
                !updatedQuestion.Answers.Any(a => a.IsCorrect) || 
                updatedQuestion.Categories.Count == 0)
            {
                throw new InvalidOperationException("Invalid question: must have exactly 3 answers, 1 correct answer and at least one category.");
            }
            
            Question? question = await context.Questions
                .Include(q => q.Answers)
                .Include(q => q.QuestionCategories)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null)
            {
                await transaction.RollbackAsync();
                return false;
            }

            question.QuestionText = updatedQuestion.QuestionText;
            question.LastUpdatedById = userId;
            question.LastUpdatedAt = DateTime.UtcNow;
            
            await answerService.UpdateQuestionAnswers(question.Id, question.Answers, updatedQuestion.Answers);
            await questionCategoriesService.UpdateQuestionCategories(question.Id, question.QuestionCategories, updatedQuestion.Categories);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException($"An error occurred while updating the question with ID {id}.", ex);
        }
    }

    public async Task<bool> DeleteQuestion(int id)
    {
        try
        {
            Question? question = await context.Questions
                .Include(a => a.Answers)
                .FirstOrDefaultAsync(a => a.Id == id);
            
            if (question == null) return false;
            
            context.Questions.Remove(question);
            await context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while deleting the question with ID {id}.", ex);
        }
    }
    
    private async Task<List<Question>> FetchRandomQuestions(IQueryable<Question> query, int count, HashSet<int>? excludeIds)
    {
        try
        {
            if (excludeIds != null) query = query.Where(q => !excludeIds.Contains(q.Id));

            return await query
                .OrderBy(q => Guid.NewGuid())
                .Take(count)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while parsing random questions.", ex);
        }
    }
    
    private List<QuestionDto> MapQuestionsToDtos(List<Question> questions, bool isSingleAnswerMode)
    {
        try
        {
            Random random = new Random();

            return questions
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Answers = q.Answers
                        .OrderBy(a => a.IsCorrect ? 0 : 1) 
                        .Take(isSingleAnswerMode ? 1 : 3) 
                        .OrderBy(a => random.Next())
                        .Select(a => new AnswerDto
                        {
                            Id = a.Id,
                            AnswerText = a.AnswerText,
                            IsCorrect = a.IsCorrect
                        })
                        .ToList()
                })
                .ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while mapping random questions", ex);
        }
    }
}
