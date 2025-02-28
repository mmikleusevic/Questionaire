using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IAnswerService
{
    Task<List<Answer>> GetAnswers();
    Task<Answer?> GetAnswerById(int id);
    Task CreateAnswer(AnswerDto answer);
    Task<bool> UpdateAnswer(int id, AnswerDto updatedAnswer);
    Task<bool> DeleteAnswer(int id);
}