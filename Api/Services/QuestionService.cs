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
        return await context.Questions.Include(a => a.Answers)
            .ToListAsync();
    }

    public async Task<Question?> GetQuestionByIdAsync(int id)
    {
        return await context.Questions.Include(a => a.Answers).FirstAsync(a => a.Id == id);
    }

    public async Task<List<QuestionDto>> GetRandomUniqueQuestions(GetRandomUniqueQuestionsRequest request)
    {
        IQueryable<Question> query = context.Questions
            .Where(q => (request.IsSingleAnswerMode ? q.Answers.Count >= 1 : q.Answers.Count >= 3) 
                        && q.Answers.Any(a => a.IsCorrect))
            .Where(q => q.QuestionCategories.Any(qc => request.CategoryIds.Contains(qc.CategoryId)))
            .Where(q => !context.UserQuestionHistory
                .Any(h => h.UserId == request.UserId && h.QuestionId == q.Id));

        if (!await query.AnyAsync()) return new List<QuestionDto>();
    
        List<QuestionDto> questions = await GetRandomQuestions(query, request.NumberOfQuestions, request.IsSingleAnswerMode);

        HashSet<int> fetchedQuestionIds = questions.Select(a => a.Id).ToHashSet();
    
        if (questions.Count < request.NumberOfQuestions)
        {
            await userQuestionHistoryService.ResetUserQuestionHistory(request.UserId);
            
            int remainingQuestionsCount = request.NumberOfQuestions - questions.Count;
            query = query.Where(q => !fetchedQuestionIds.Contains(q.Id));
            
            List<QuestionDto> additionalQuestions = await GetRandomQuestions(query, remainingQuestionsCount, request.IsSingleAnswerMode);
            questions.AddRange(additionalQuestions);
        }
    
        await userQuestionHistoryService.SaveUserQuestionHistory(request.UserId, questions);

        return questions;
    }
    
    private async Task<List<QuestionDto>> GetRandomQuestions(IQueryable<Question> query, int count, bool isSingleAnswerMode)
    {
        return await query
            .OrderBy(q => Guid.NewGuid())
            .Take(count)
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                Answers = q.Answers
                    .OrderBy(a => a.IsCorrect ? 0 : 1)
                    .Take(isSingleAnswerMode ? 1 : 3)
                    .OrderBy(a => Guid.NewGuid())
                    .Select(a => new AnswerDto
                    {
                        Id = a.Id,
                        AnswerText = a.AnswerText,
                        IsCorrect = a.IsCorrect
                    })
                    .ToList()
            })
            .ToListAsync();
    }

    public async Task<Question> AddQuestionAsync(Question question)
    {
        context.Questions.Add(question);
        await context.SaveChangesAsync();
        return question;
    }

    public async Task<bool> UpdateQuestionAsync(int id, Question updatedQuestion)
    {
        Question question = await context.Questions.FindAsync(id);
        if (question == null) return false;

        question.QuestionText = updatedQuestion.QuestionText;
        
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteQuestionAsync(int id)
    {
        Question question = await context.Questions.FindAsync(id);
        if (question == null) return false;

        context.Questions.Remove(question);
        await context.SaveChangesAsync();
        return true;
    }
}