using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using Shared.Models;

namespace QuestionaireApi.Services;

public class PendingAnswerService(QuestionaireDbContext context) : IPendingAnswerService
{
    public async Task CreatePendingQuestionAnswers(int pendingQuestionId, List<PendingAnswerDto> pendingAnswers)
    {
        try
        {
            await context.PendingAnswers.AddRangeAsync(
                pendingAnswers.Select(a => new PendingAnswer
                {
                    AnswerText = a.AnswerText,
                    IsCorrect = a.IsCorrect,
                    PendingQuestionId = pendingQuestionId
                }).ToList()
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while creating the pending question answers.", ex);
        }
    }

    public Task UpdatePendingQuestionAnswers(int pendingQuestionId, ICollection<PendingAnswer> pendingAnswers,
        List<PendingAnswerDto> updatedPendingAnswers)
    {
        try
        {
            List<PendingAnswer> pendingAnswersToRemove = pendingAnswers
                .Where(pa => updatedPendingAnswers.All(ua => ua.Id != pa.Id))
                .ToList();

            foreach (PendingAnswer pendingAnswer in pendingAnswersToRemove)
            {
                pendingAnswers.Remove(pendingAnswer);
            }

            foreach (PendingAnswerDto updatedPendingAnswer in updatedPendingAnswers)
            {
                PendingAnswer? existingAnswer = pendingAnswers.Where(a => a.Id != 0)
                    .FirstOrDefault(pa => pa.Id == updatedPendingAnswer.Id);

                if (existingAnswer != null)
                {
                    existingAnswer.AnswerText = updatedPendingAnswer.AnswerText;
                    existingAnswer.IsCorrect = updatedPendingAnswer.IsCorrect;
                }
                else
                {
                    pendingAnswers.Add(new PendingAnswer
                    {
                        AnswerText = updatedPendingAnswer.AnswerText,
                        IsCorrect = updatedPendingAnswer.IsCorrect,
                        PendingQuestionId = pendingQuestionId
                    });
                }
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(
                new InvalidOperationException("An error occurred while updating the pending question answers.", ex));
        }
    }
}