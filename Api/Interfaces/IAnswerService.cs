using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IAnswerService
{
    Task<List<Answer>> GetAnswers();
    Task<Answer?> GetAnswerById(int id);
    Task CreateAnswer(Answer answer);
    Task<bool> UpdateAnswer(int id, Answer updatedAnswer);
    Task<bool> DeleteAnswer(int id);
}