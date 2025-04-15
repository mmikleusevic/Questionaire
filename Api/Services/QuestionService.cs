using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using Shared.Models;
using SharedStandard.Models;

namespace QuestionaireApi.Services;

public class QuestionService(
    QuestionaireDbContext context,
    IUserQuestionHistoryService userQuestionHistoryService,
    IAnswerService answerService,
    IQuestionCategoriesService questionCategoriesService,
    UserManager<User> userManager) : IQuestionService
{
    private const string SuperAdminRole = "SuperAdmin";
    private const string AdminRole = "Admin";

    public async Task<PaginatedResponse<QuestionExtendedDto>> GetQuestions(QuestionsRequestDto questionsRequestDto,
        ClaimsPrincipal user)
    {
        try
        {
            User? userDb = await userManager.GetUserAsync(user);

            if (userDb == null)
                return new PaginatedResponse<QuestionExtendedDto> { Items = new List<QuestionExtendedDto>() };

            IQueryable<Question> baseQuery = context.Questions
                .Where(q => q.IsDeleted == false)
                .Where(q => q.IsApproved == questionsRequestDto.FetchApprovedQuestions);

            if (questionsRequestDto.OnlyMyQuestions)
            {
                baseQuery = baseQuery.Where(q => q.CreatedById == userDb.Id);
            }

            if (!string.IsNullOrEmpty(questionsRequestDto.SearchQuery))
            {
                string searchTerm = $"%{questionsRequestDto.SearchQuery.ToLower()}%";
                baseQuery = baseQuery.Where(q => EF.Functions.Like(q.QuestionText.ToLower(), searchTerm));
            }

            int totalQuestions = await baseQuery.CountAsync();

            List<Question> questions = await baseQuery
                .Include(q => q.Answers)
                .Include(q => q.QuestionCategories)
                .ThenInclude(qc => qc.Category)
                .OrderBy(q => q.Id)
                .Skip((questionsRequestDto.PageNumber - 1) * questionsRequestDto.PageSize)
                .Take(questionsRequestDto.PageSize)
                .AsSplitQuery()
                .ToListAsync();

            PaginatedResponse<QuestionExtendedDto> response = new PaginatedResponse<QuestionExtendedDto>
            {
                Items = questions.Select(q => new QuestionExtendedDto(q.Id)
                {
                    QuestionText = q.QuestionText,
                    CreatedById = q.CreatedById,
                    Difficulty = q.Difficulty,
                    Answers = q.Answers.Select(a => new AnswerExtendedDto(a.Id)
                    {
                        AnswerText = a.AnswerText,
                        IsCorrect = a.IsCorrect
                    }).ToList(),
                    Categories = q.QuestionCategories.Select(qc => new CategoryExtendedDto(qc.Category.Id)
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

    public async Task<List<QuestionExtendedDto>> GetRandomUniqueQuestions(UniqueQuestionsRequestDto requestDto)
    {
        try
        {
            IQueryable<Question> coreQuery = context.Questions
                .Where(q => q.IsApproved == true && q.IsDeleted == false)
                .Where(q => requestDto.Difficulties.Contains(q.Difficulty))
                .Where(q => q.QuestionCategories.Any(qc => requestDto.CategoryIds.Contains(qc.CategoryId)))
                .Where(q => q.Answers.Any(a => a.IsCorrect))
                .Include(q => q.Answers);

            IQueryable<Question> initialQuery = coreQuery
                .Where(q => !context.UserQuestionHistory.Any(h => h.UserId == requestDto.UserId
                                                                  && h.QuestionId == q.Id));

            List<Question> questions = await FetchRandomQuestions(initialQuery, requestDto.NumberOfQuestions, null);

            if (questions.Count < requestDto.NumberOfQuestions)
            {
                await userQuestionHistoryService.ResetUserQuestionHistory(requestDto.UserId);

                int remainingQuestionsCount = requestDto.NumberOfQuestions - questions.Count;
                HashSet<int> idsToExclude = questions.Select(q => q.Id).ToHashSet();

                List<Question> additionalQuestions =
                    await FetchRandomQuestions(coreQuery, remainingQuestionsCount, idsToExclude);

                questions.AddRange(additionalQuestions);
            }

            return MapQuestionsToDtos(questions, requestDto.IsSingleAnswerMode);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving the random questions.", ex);
        }
    }

    public async Task<bool> ApproveQuestion(int id, ClaimsPrincipal user)
    {
        try
        {
            string? userId = userManager.GetUserId(user);

            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("The user is not authorized.");

            Question? question = await context.Questions
                .Include(q => q.Answers)
                .Include(q => q.QuestionCategories)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (question == null || question.IsApproved) return false;

            if (question.Answers == null ||
                question.Answers.Count != 3 ||
                !question.Answers.Any(a => a.IsCorrect) ||
                question.QuestionCategories == null ||
                question.QuestionCategories.Count == 0)
            {
                throw new InvalidOperationException(
                    "Invalid question: must have exactly 3 answers, 2 incorrect answers, 1 correct answer and at least one category.");
            }

            bool isOwner = question.CreatedById == userId;
            bool isAdmin = user.IsInRole(AdminRole);
            bool isSuperAdmin = user.IsInRole(SuperAdminRole);

            if (!isAdmin && !isSuperAdmin) return false;
            if (isAdmin && !isSuperAdmin && isOwner) return false;

            question.ApprovedAt = DateTime.UtcNow;
            question.IsApproved = true;
            question.ApprovedById = userId;

            context.Questions.Update(question);
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while approving the question.", ex);
        }
    }

    public async Task CreateQuestion(QuestionExtendedDto newQuestion, ClaimsPrincipal user)
    {
        IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

        try
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using IDbContextTransaction? transaction = await context.Database.BeginTransactionAsync();

                string? userId = userManager.GetUserId(user);

                if (string.IsNullOrEmpty(userId))
                {
                    await transaction.RollbackAsync();

                    throw new UnauthorizedAccessException("The user is not authorized");
                }

                if (newQuestion.Answers == null ||
                    newQuestion.Answers.Count != 3 ||
                    !newQuestion.Answers.Any(a => a.IsCorrect) ||
                    newQuestion.Categories == null ||
                    newQuestion.Categories.Count == 0)
                {
                    await transaction.RollbackAsync();

                    throw new InvalidOperationException(
                        "Invalid question: must have exactly 3 answers, 2 incorrect answers, 1 correct answer and at least one category.");
                }

                Question dbQuestion = new Question
                {
                    QuestionText = newQuestion.QuestionText,
                    CreatedById = userId,
                    CreatedAt = DateTime.UtcNow,
                    IsApproved = false,
                    Difficulty = newQuestion.Difficulty
                };

                await context.Questions.AddAsync(dbQuestion);
                await context.SaveChangesAsync();

                await answerService.CreateQuestionAnswers(dbQuestion.Id, newQuestion.Answers);
                await questionCategoriesService.CreateQuestionCategories(dbQuestion.Id,
                    newQuestion.Categories);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                if (user.IsInRole(SuperAdminRole))
                {
                    bool approved = await ApproveQuestion(dbQuestion.Id, user);
                    if (!approved)
                    {
                        throw new InvalidOperationException(
                            $"Auto approval failed for question {dbQuestion.Id} created by SuperAdmin {userId}.");
                    }
                }
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while creating the question.", ex);
        }
    }

    public async Task<bool> UpdateQuestion(int id, QuestionExtendedDto updatedQuestion, ClaimsPrincipal user)
    {
        IExecutionStrategy strategy = context.Database.CreateExecutionStrategy();

        try
        {
            return await strategy.ExecuteAsync(async () =>
            {
                await using IDbContextTransaction? transaction = await context.Database.BeginTransactionAsync();

                string? userId = userManager.GetUserId(user);

                if (string.IsNullOrEmpty(userId))
                {
                    await transaction.RollbackAsync();
                    throw new UnauthorizedAccessException("The user is not authorized");
                }

                if (updatedQuestion.Answers == null ||
                    updatedQuestion.Answers.Count != 3 ||
                    !updatedQuestion.Answers.Any(a => a.IsCorrect) ||
                    updatedQuestion.Categories == null ||
                    updatedQuestion.Categories.Count == 0)
                {
                    await transaction.RollbackAsync();

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
                question.Difficulty = updatedQuestion.Difficulty;

                await answerService.UpdateQuestionAnswers(question.Id, question.Answers, updatedQuestion.Answers);
                await questionCategoriesService.UpdateQuestionCategories(question.Id, question.QuestionCategories,
                    updatedQuestion.Categories);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            });
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"An error occurred while updating the question with ID {id}.", ex);
        }
    }

    public async Task<bool> DeleteQuestion(int id, ClaimsPrincipal user)
    {
        try
        {
            string? userId = userManager.GetUserId(user);

            if (string.IsNullOrEmpty(userId)) return false;

            Question? question = await context.Questions
                .Include(a => a.Answers)
                .FirstOrDefaultAsync(q => q.Id == id && q.IsDeleted == false);

            if (question == null) return false;

            bool isAdmin = user.IsInRole(AdminRole);
            bool isSuperAdmin = user.IsInRole(SuperAdminRole);
            bool isOwner = question.CreatedById == userId;

            if (!isAdmin && !isSuperAdmin && !isOwner) return false;

            question.DeletedAt = DateTime.UtcNow;
            question.DeletedById = userId;
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
            if (excludeIds != null && excludeIds.Any())
            {
                query = query.Where(q => !excludeIds.Contains(q.Id));
            }

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

    private List<QuestionExtendedDto> MapQuestionsToDtos(List<Question> questions, bool isSingleAnswerMode)
    {
        try
        {
            Random random = new Random();

            return questions
                .Select(q => new QuestionExtendedDto(q.Id)
                {
                    QuestionText = q.QuestionText,
                    Difficulty = q.Difficulty,
                    Answers = q.Answers
                        .OrderBy(a => random.Next())
                        .Select(a => new AnswerExtendedDto(a.Id)
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