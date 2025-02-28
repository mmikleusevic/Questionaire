using QuestionaireApi.Models;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IQuestionService
{
    Task<PaginatedResponse<QuestionDto>> GetQuestions(int pageNumber, int pageSize);
    Task<Question?> GetQuestionById(int id);
    Task<List<QuestionDto>> GetRandomUniqueQuestions(GetRandomUniqueQuestionsRequestDto requestDto);
    Task<bool> UpdateQuestion(int id, QuestionDto updatedQuestion);
    Task<bool> DeleteQuestion(int id);
}