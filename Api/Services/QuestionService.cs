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

    public async Task<List<QuestionDto>> GetRandomUniqueQuestions(string userId, int numberOfQuestions)
    {
        int currentRound = await context.UserQuestionHistory
            .Where(h => h.UserId == userId)
            .Select(h => (int?)h.RoundNumber)
            .MaxAsync() ?? 0;

        List<QuestionDto> questions = await context.Questions
            .Where(q => !context.UserQuestionHistory
                .Where(h => h.UserId == userId && h.RoundNumber == currentRound)
                .Select(h => h.QuestionId)
                .Contains(q.Id))
            .OrderBy(q => Guid.NewGuid())
            .Take(numberOfQuestions)
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

        if (questions.Count < numberOfQuestions)
        {
            currentRound++;
            int remainingCount = numberOfQuestions - questions.Count;

            List<QuestionDto> additionalQuestions = await context.Questions
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