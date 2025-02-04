using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Services;

public class QuestionService(QuestionaireDbContext context) : IQuestionService
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
        List<int> answeredQuestionIds = await context.UserQuestionHistory
            .Where(h => h.UserId == request.UserId)
            .Select(h => h.QuestionId)
            .ToListAsync();

        IQueryable<Question> query = context.Questions.AsQueryable();
        
        if (request.CategoryIds != null && request.CategoryIds.Any())
        {
            query = query.Where(q => request.CategoryIds.Contains(q.CategoryId));
        }

        List<QuestionDto> questions = await query
            .Where(q => !answeredQuestionIds.Contains(q.Id))
            .OrderBy(q => Guid.NewGuid())
            .Take(request.NumberOfQuestions)
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText,
                Answers = q.Answers
                    .OrderBy(a => Guid.NewGuid())
                    .Take(3)
                    .Select(a => new AnswerDto
                    {
                        Id = a.Id,
                        AnswerText = a.AnswerText,
                        IsCorrect = a.IsCorrect
                    })
                    .ToList()
            })
            .ToListAsync();

        if (questions.Count < request.NumberOfQuestions)
        {
            List<UserQuestionHistory> historyToRemove = await context.UserQuestionHistory
                .Where(h => h.UserId == request.UserId)
                .ToListAsync();
            
            context.UserQuestionHistory.RemoveRange(historyToRemove);
            await context.SaveChangesAsync();
            
            int remainingCount = request.NumberOfQuestions - questions.Count;
            HashSet<int> existingIds = questions.Select(q => q.Id).ToHashSet();

            List<QuestionDto> additionalQuestions = await query
                .Where(q => !existingIds.Contains(q.Id))
                .OrderBy(q => Guid.NewGuid())
                .Take(remainingCount)
                .Select(q => new QuestionDto
                {
                    Id = q.Id,
                    QuestionText = q.QuestionText,
                    Answers = q.Answers
                        .OrderBy(a => Guid.NewGuid())
                        .Take(3)
                        .Select(a => new AnswerDto
                        {
                            Id = a.Id,
                            AnswerText = a.AnswerText,
                            IsCorrect = a.IsCorrect
                        })
                        .ToList()
                })
                .ToListAsync();

            questions.AddRange(additionalQuestions);
        }
        
        IEnumerable<UserQuestionHistory> newHistory = questions.Select(q => new UserQuestionHistory
        {
            UserId = request.UserId,
            QuestionId = q.Id,
            RoundNumber = 1
        });

        await context.UserQuestionHistory.AddRangeAsync(newHistory);
        await context.SaveChangesAsync();

        return questions;
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