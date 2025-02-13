using System.Diagnostics;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Services;

public class QuestionService(QuestionaireDbContext context, 
    IUserQuestionHistoryService userQuestionHistoryService) : IQuestionService
{
    public async Task<List<Question>> GetQuestionsAsync()
    {
        try
        {
            return await context.Questions.Include(a => a.Answers)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new ApplicationException("An error occurred while retrieving the list of questions.", ex);
        }
    }

    public async Task<Question?> GetQuestionByIdAsync(int id)
    {
        try
        {
            return await context.Questions.Include(a => a.Answers)
                .FirstOrDefaultAsync(a => a.Id == id);
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while retrieving the question with ID {id}.", ex);
        }
    }
    
    public async Task<List<QuestionDto>> GetRandomUniqueQuestions(GetRandomUniqueQuestionsRequest request)
    {
        IQueryable<Question> baseQuery = context.Questions
            .Include(q => q.Answers)
            .Where(q => q.Answers.Count >= (request.IsSingleAnswerMode ? 1 : 3))
            .Where(q => q.Answers.Any(a => a.IsCorrect))
            .Where(q => q.QuestionCategories.Any(qc => request.CategoryIds.Contains(qc.CategoryId)))
            .Where(q => !context.UserQuestionHistory.Any(h => h.UserId == request.UserId && h.QuestionId == q.Id));
        
        List<Question> questions = await FetchRandomQuestions(baseQuery, request.NumberOfQuestions, null);
        
        if (questions.Count < request.NumberOfQuestions)
        {
            await userQuestionHistoryService.ResetUserQuestionHistory(request.UserId);

            int remainingQuestionsCount = request.NumberOfQuestions - questions.Count;
            HashSet<int> idsToExclude = questions.Select(q => q.Id).ToHashSet();
            List<Question> additionalQuestions = await FetchRandomQuestions(baseQuery, remainingQuestionsCount, idsToExclude);

            questions.AddRange(additionalQuestions);
        }
        
        await userQuestionHistoryService.CreateUserQuestionHistory(request.UserId, questions);
        
        return MapQuestionsToDtos(questions, request.IsSingleAnswerMode);
    }

    private async Task<List<Question>> FetchRandomQuestions(IQueryable<Question> query, int count, HashSet<int>? excludeIds)
    {
        if (excludeIds != null) query = query.Where(q => !excludeIds.Contains(q.Id));

        return await query
            .OrderBy(q => Guid.NewGuid())
            .Take(count)
            .ToListAsync();
    }

    private List<QuestionDto> MapQuestionsToDtos(List<Question> questions, bool isSingleAnswerMode)
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
    
    public async Task CreateQuestionAsync(Question question)
    {
        try
        {
            context.Questions.Add(question);
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while creating the question.", ex);
        }
    }

    public async Task<bool> UpdateQuestionAsync(int id, QuestionDto updatedQuestion)
    {
        try
        {
            Question? question = await context.Questions.FindAsync(id);
            if (question == null) return false;

            question.QuestionText = updatedQuestion.QuestionText;
            await context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while updating the question with ID {id}.", ex);
        }
    }

    public async Task<bool> DeleteQuestionAsync(int id)
    {
        try
        {
            Question? question = await context.Questions.FirstOrDefaultAsync(a => a.Id == id);
            if (question == null) return false;

            context.Questions.Remove(question);
            await context.SaveChangesAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"An error occurred while deleting the question with ID {id}.", ex);
        }
    }
}
