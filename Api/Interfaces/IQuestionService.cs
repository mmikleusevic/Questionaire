using System.Security.Claims;
using Shared.Models;
using UniqueQuestionsRequestDto = SharedStandard.Models.UniqueQuestionRequestDto;

namespace QuestionaireApi.Interfaces;

public interface IQuestionService
{
    Task<PaginatedResponse<QuestionValidationDto>> GetQuestions(QuestionsRequestDto questionsRequestDto,
        ClaimsPrincipal user);

    Task<List<QuestionValidationDto>> GetRandomUniqueQuestions(UniqueQuestionsRequestDto requestDto);
    Task<bool> UpdateQuestion(int id, QuestionValidationDto updatedQuestion, ClaimsPrincipal user);
    Task<bool> DeleteQuestion(int id, ClaimsPrincipal user);
}