using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IQuestionService
{
    Task<List<Question>> GetQuestionsAsync();
    Task<Question?> GetQuestionByIdAsync(int id);
    Task<List<QuestionDto>> GetRandomUniqueQuestions(string userId, int numberOfQuestions);
    Task<Question> AddQuestionAsync(Question question);
    Task<bool> UpdateQuestionAsync(int id, Question updatedQuestion);
    Task<bool> DeleteQuestionAsync(int id);
}