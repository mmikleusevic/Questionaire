using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using Shared.Models;
using UniqueQuestionsRequestDto = SharedStandard.Models.UniqueQuestionRequestDto;

namespace QuestionaireApi.Services;

public class QuestionService(
    QuestionaireDbContext context,
    IUserQuestionHistoryService userQuestionHistoryService,
    IAnswerService answerService,
    IQuestionCategoriesService questionCategoriesService,
    UserManager<User> userManager) : IQuestionService
{
    public async Task<PaginatedResponse<QuestionValidationDto>> GetQuestions(QuestionsRequestDto questionsRequestDto,
        ClaimsPrincipal user)
    {
        try
        {
            User? userDb = await userManager.GetUserAsync(user);

            if (userDb == null)
                return new PaginatedResponse<QuestionValidationDto> { Items = new List<QuestionValidationDto>() };

            IQueryable<Question> query = context.Questions
                .Include(a => a.Answers)
                .Include(a => a.QuestionCategories)
                .ThenInclude(c => c.Category)
                .OrderBy(q => q.Id);

            if (questionsRequestDto.OnlyMyQuestions)
            {
                query = query.Where(q => q.CreatedById == userDb.Id);
            }

            List<Question> questions = await query
                .Skip((questionsRequestDto.PageNumber - 1) * questionsRequestDto.PageSize)
                .Take(questionsRequestDto.PageSize)
                .ToListAsync();

            int totalQuestions = await query.CountAsync();

            PaginatedResponse<QuestionValidationDto> response = new PaginatedResponse<QuestionValidationDto>
            {
                Items = questions.Select(q => new QuestionValidationDto(q.Id)
                {
                    QuestionText = q.QuestionText,
                    Answers = q.Answers.Select(a => new AnswerValidationDto(a.Id)
                    {
                        AnswerText = a.AnswerText,
                        IsCorrect = a.IsCorrect
                    }).ToList(),
                    Categories = q.QuestionCategories.Select(qc => new CategoryValidationDto(qc.Category.Id)
                    {
                        CategoryName = qc.Category.CategoryName
                    }).ToList()
                }).ToList(),
                TotalCount = totalQuestions,
                PageSize = questionsRequestDto.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalQuestions / questionsRequestDto.PageSize)
            };

            return response;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving the list of questions.", ex);
        }
    }

    public async Task<List<QuestionValidationDto>> GetRandomUniqueQuestions(UniqueQuestionsRequestDto requestDto)
    {
        try
        {
            IQueryable<Question> baseQuery = context.Questions
                .Include(q => q.Answers)
                .Where(q => q.Answers.Count >= (requestDto.IsSingleAnswerMode ? 1 : 3))
                .Where(q => q.Answers.Any(a => a.IsCorrect))
                .Where(q => q.QuestionCategories.Any(qc => requestDto.CategoryIds.Contains(qc.CategoryId)))
                .Where(q => !context.UserQuestionHistory.Any(h => h.UserId == requestDto.UserId
                                                                  && h.QuestionId == q.Id));

            List<Question> questions = await FetchRandomQuestions(baseQuery, requestDto.NumberOfQuestions, null);

            if (questions.Count < requestDto.NumberOfQuestions)
            {
                await userQuestionHistoryService.ResetUserQuestionHistory(requestDto.UserId);

                int remainingQuestionsCount = requestDto.NumberOfQuestions - questions.Count;
                HashSet<int> idsToExclude = questions.Select(q => q.Id).ToHashSet();
                List<Question> additionalQuestions =
                    await FetchRandomQuestions(baseQuery, remainingQuestionsCount, idsToExclude);

                questions.AddRange(additionalQuestions);
            }

            await userQuestionHistoryService.CreateUserQuestionHistory(requestDto.UserId, questions);

            return MapQuestionsToDtos(questions, requestDto.IsSingleAnswerMode);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving the random questions.", ex);
        }
    }

    public async Task<bool> UpdateQuestion(int id, QuestionValidationDto updatedQuestion, ClaimsPrincipal user)
    {
        await using IDbContextTransaction? transaction = await context.Database.BeginTransactionAsync();

        try
        {
            string? userId = userManager.GetUserId(user);

            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("The user is not authorized");

            if (updatedQuestion.Answers.Count != 3 ||
                !updatedQuestion.Answers.Any(a => a.IsCorrect) ||
                updatedQuestion.Categories.Count == 0)
            {
                throw new InvalidOperationException(
                    "Invalid question: must have exactly 3 answers, 1 correct answer and at least one category.");
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
            await questionCategoriesService.UpdateQuestionCategories(question.Id, question.QuestionCategories,
                updatedQuestion.Categories);

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

    public async Task<bool> DeleteQuestion(int id, ClaimsPrincipal user)
    {
        try
        {
            string? userId = userManager.GetUserId(user);

            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("The user is not authorized");

            Question? question = await context.Questions
                .Include(a => a.Answers)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (question == null) return false;

            question.IsDeleted = true;
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while deleting the question with ID {id}.", ex);
        }
    }

    private async Task<List<Question>> FetchRandomQuestions(IQueryable<Question> query, int count,
        HashSet<int>? excludeIds)
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
            throw new InvalidOperationException("An error occurred while parsing random questions.", ex);
        }
    }

    private List<QuestionValidationDto> MapQuestionsToDtos(List<Question> questions, bool isSingleAnswerMode)
    {
        try
        {
            Random random = new Random();

            return questions
                .Select(q => new QuestionValidationDto(q.Id)
                {
                    QuestionText = q.QuestionText,
                    Answers = q.Answers
                        .OrderBy(a => a.IsCorrect ? 0 : 1)
                        .Take(isSingleAnswerMode ? 1 : 3)
                        .OrderBy(a => random.Next())
                        .Select(a => new AnswerValidationDto(a.Id)
                        {
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