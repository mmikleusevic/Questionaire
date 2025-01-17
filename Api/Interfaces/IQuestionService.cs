using QuestionaireApi.Models;

namespace QuestionaireApi.Interfaces;

public interface IQuestionService
{
    Task<List<Question>> GetQuestionsAsync();
    Task<Question?> GetQuestionByIdAsync(int id);
    Task<List<Question>> GetRandomUniqueQuestions(int userId, int numberOfQuestions);
    Task<Question> AddQuestionAsync(Question question);
    Task<bool> UpdateQuestionAsync(int id, Question updatedQuestion);
    Task<bool> DeleteQuestionAsync(int id);
}