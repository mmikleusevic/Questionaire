using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IAnswerService
{
    Task<List<Answer>> GetAnswersAsync();
    Task<Answer?> GetAnswerByIdAsync(int id);
    Task CreateAnswerAsync(Answer answer);
    Task<bool> UpdateAnswerAsync(int id, Answer updatedAnswer);
    Task<bool> DeleteAnswerAsync(int id);
}