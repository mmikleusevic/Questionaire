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
    public async Task<PaginatedResponse<QuestionExtendedDto>> GetQuestions(QuestionsRequestDto questionsRequestDto,
        ClaimsPrincipal user)
    {
        try
        {
            User? userDb = await userManager.GetUserAsync(user);

            if (userDb == null)
                return new PaginatedResponse<QuestionExtendedDto> { Items = new List<QuestionExtendedDto>() };

            IQueryable<Question> query = context.Questions
                .Where(q => q.IsDeleted == false)
                .Include(a => a.Answers)
                .Include(a => a.QuestionCategories)
                .ThenInclude(c => c.Category)
                .Where(q => q.IsApproved == questionsRequestDto.FetchApprovedQuestions)
                .OrderBy(q => q.Id);

            if (questionsRequestDto.OnlyMyQuestions)
            {
                query = query.Where(q => q.CreatedById == userDb.Id);
            }

            if (!string.IsNullOrEmpty(questionsRequestDto.SearchQuery))
            {
                query = query.Where(q => EF.Functions.Like(q.QuestionText, $"%{questionsRequestDto.SearchQuery}%"));
            }

            int totalQuestions = await query.CountAsync();

            List<Question> questions = await query
                .Skip((questionsRequestDto.PageNumber - 1) * questionsRequestDto.PageSize)
                .Take(questionsRequestDto.PageSize)
                .ToListAsync();

            PaginatedResponse<QuestionExtendedDto> response = new PaginatedResponse<QuestionExtendedDto>
            {
                Items = questions.Select(q => new QuestionExtendedDto(q.Id)
                {
                    QuestionText = q.QuestionText,
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
                .Include(q => q.Answers)
                .Where(q => q.Answers.Count >= (requestDto.IsSingleAnswerMode ? 1 : 3))
                .Where(q => q.Answers.Any(a => a.IsCorrect))
                .Where(q => q.QuestionCategories.Any(qc => requestDto.CategoryIds.Contains(qc.CategoryId)));

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
            
            if (questions.Any())
            {
                await userQuestionHistoryService.CreateUserQuestionHistory(requestDto.UserId, questions);
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

            if (question == null) return false;

            if (question.Answers.Count != 3 ||
                !question.Answers.Any(a => a.IsCorrect) ||
                question.QuestionCategories.Count == 0)
            {
                throw new InvalidOperationException(
                    "Invalid question: must have exactly 3 answers, 2 incorrect answers, 1 correct answer and at least one category.");
            }

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
        await using IDbContextTransaction? transaction = await context.Database.BeginTransactionAsync();

        try
        {
            string? userId = userManager.GetUserId(user);

            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("The user is not authorized");

            if (newQuestion.Answers.Count != 3 ||
                !newQuestion.Answers.Any(a => a.IsCorrect) ||
                newQuestion.Categories.Count == 0)
            {
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
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("An error occurred while creating the question.", ex);
        }
    }

    public async Task<bool> UpdateQuestion(int id, QuestionExtendedDto updatedQuestion, ClaimsPrincipal user)
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
            question.Difficulty = updatedQuestion.Difficulty;

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

            if (string.IsNullOrEmpty(userId)) return false;

            Question? question = await context.Questions
                .Include(a => a.Answers)
                .FirstOrDefaultAsync(q => q.Id == id && q.IsDeleted == false);

            if (question == null) return false;

            bool isAdmin = user.IsInRole("Admin");
            bool isSuperAdmin = user.IsInRole("SuperAdmin");
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
                        .OrderBy(a => a.IsCorrect ? 0 : 1)
                        .Take(isSingleAnswerMode ? 1 : 3)
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