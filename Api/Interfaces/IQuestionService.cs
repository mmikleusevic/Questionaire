using System.Security.Claims;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IQuestionService
{
    Task<PaginatedResponse<QuestionDto>> GetQuestions(QuestionsRequestDto questionsRequestDto, ClaimsPrincipal user);
    Task<List<QuestionDto>> GetRandomUniqueQuestions(GetRandomUniqueQuestionsRequestDto requestDto);
    Task<bool> UpdateQuestion(int id, QuestionDto updatedQuestion, ClaimsPrincipal user);
    Task<bool> DeleteQuestion(int id, ClaimsPrincipal user);
}