using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IQuestionService
{
    Task<PaginatedResponse<QuestionDto>> GetQuestionsAsync(int pageNumber, int pageSize);
    Task<Question?> GetQuestionByIdAsync(int id);
    Task<List<QuestionDto>> GetRandomUniqueQuestions(GetRandomUniqueQuestionsRequestDto requestDto);
    Task CreateQuestionAsync(Question question);
    Task<bool> UpdateQuestionAsync(int id, QuestionDto updatedQuestion);
    Task<bool> DeleteQuestionAsync(int id);
}