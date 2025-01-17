using QuestionaireApi.Models;

namespace QuestionaireApi.Interfaces;

public interface IAnswerService
{
    Task<List<Answer>> GetAnswersAsync();
    Task<Answer?> GetAnswerByIdAsync(int id);
    Task<Answer> AddAnswerAsync(Answer answer);
    Task<bool> UpdateAnswerAsync(int id, Answer updatedAnswer);
    Task<bool> DeleteAnswerAsync(int id);
}