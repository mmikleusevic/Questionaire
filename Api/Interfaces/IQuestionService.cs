using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IQuestionService
{
    Task<List<Question>> GetQuestionsAsync();
    Task<Question?> GetQuestionByIdAsync(int id);
    Task<List<QuestionDto>> GetRandomUniqueQuestions(GetRandomUniqueQuestionsRequest request);
    Task CreateQuestionAsync(Question question);
    Task<bool> UpdateQuestionAsync(int id, QuestionDto updatedQuestion);
    Task<bool> DeleteQuestionAsync(int id);
}