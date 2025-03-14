using System.Security.Claims;
using Shared.Models;
using QuestionDto = Shared.Models.QuestionDto;
using UniqueQuestionsRequestDto = SharedStandard.Models.UniqueQuestionRequestDto;

namespace QuestionaireApi.Interfaces;

public interface IQuestionService
{
    Task<PaginatedResponse<QuestionDto>> GetQuestions(QuestionsRequestDto questionsRequestDto, ClaimsPrincipal user);
    Task<List<QuestionDto>> GetRandomUniqueQuestions(UniqueQuestionsRequestDto requestDto);
    Task<bool> UpdateQuestion(int id, QuestionDto updatedQuestion, ClaimsPrincipal user);
    Task<bool> DeleteQuestion(int id, ClaimsPrincipal user);
}