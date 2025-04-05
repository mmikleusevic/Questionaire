using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using Shared.Models;

namespace QuestionaireApi.Services;

public class AnswerService(QuestionaireDbContext context) : IAnswerService
{
    public async Task CreateQuestionAnswers(int questionId, List<AnswerExtendedDto> answers)
    {
        try
        {
            if (answers.Count == 0) return;

            await context.Answers.AddRangeAsync(
                answers.Select(a => new Answer
                {
                    AnswerText = a.AnswerText,
                    IsCorrect = a.IsCorrect,
                    QuestionId = questionId
                }).ToList()
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while updating the question answers.", ex);
        }
    }

    public Task UpdateQuestionAnswers(int questionId, ICollection<Answer> answers,
        List<AnswerExtendedDto> updatedAnswers)
    {
        try
        {
            List<Answer> answersToRemove = answers
                .Where(pa => updatedAnswers.All(ua => ua.Id != pa.Id))
                .ToList();

            foreach (Answer answer in answersToRemove)
            {
                answers.Remove(answer);
            }

            foreach (AnswerExtendedDto updatedAnswer in updatedAnswers)
            {
                Answer? existingAnswer = answers.Where(a => a.Id != 0)
                    .FirstOrDefault(pa => pa.Id == updatedAnswer.Id);

                if (existingAnswer != null)
                {
                    existingAnswer.AnswerText = updatedAnswer.AnswerText;
                    existingAnswer.IsCorrect = updatedAnswer.IsCorrect;
                }
                else
                {
                    answers.Add(new Answer
                    {
                        AnswerText = updatedAnswer.AnswerText,
                        IsCorrect = updatedAnswer.IsCorrect,
                        QuestionId = questionId
                    });
                }
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(
                new InvalidOperationException("An error occurred while updating the question answers.", ex));
        }
    }
}