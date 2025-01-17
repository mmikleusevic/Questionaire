using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models;

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

    public async Task<List<Question>> GetRandomUniqueQuestions(int userId, int numberOfQuestions)
    {
        int currentRound = await context.UserQuestionHistory
            .Where(h => h.UserId == userId)
            .Select(h => (int?)h.RoundNumber)
            .MaxAsync() ?? 0;
        
        List<Question> questions = await context.Questions
            .Where(q => !context.UserQuestionHistory
                .Where(h => h.UserId == userId && h.RoundNumber == currentRound)
                .Select(h => h.QuestionId)
                .Contains(q.Id))
            .OrderBy(q => Guid.NewGuid()) 
            .Take(numberOfQuestions)
            .Include(q => q.Answers)
            .ToListAsync();
        
        if (questions.Count < numberOfQuestions)
        {
            currentRound++;
            int remainingCount = numberOfQuestions - questions.Count;
        
            List<Question> additionalQuestions = await context.Questions
                .OrderBy(q => Guid.NewGuid())
                .Take(remainingCount)
                .Include(q => q.Answers)
                .ToListAsync();
        
            questions.AddRange(additionalQuestions);
        }
        
        IEnumerable<UserQuestionHistory> history = questions.Select(q => new UserQuestionHistory
        {
            UserId = userId,
            QuestionId = q.Id,
            RoundNumber = currentRound
        });
    
        await context.UserQuestionHistory.AddRangeAsync(history);
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