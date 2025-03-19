using System.Security.Claims;
using Shared.Models;
using UniqueQuestionsRequestDto = SharedStandard.Models.UniqueQuestionRequestDto;

namespace QuestionaireApi.Interfaces;

public interface IQuestionService
{
    Task<PaginatedResponse<QuestionExtendedDto>> GetQuestions(QuestionsRequestDto questionsRequestDto,
        ClaimsPrincipal user);

    Task<List<QuestionExtendedDto>> GetRandomUniqueQuestions(UniqueQuestionsRequestDto requestDto);
    Task<bool> UpdateQuestion(int id, QuestionExtendedDto updatedQuestion, ClaimsPrincipal user);
    Task<bool> DeleteQuestion(int id, ClaimsPrincipal user);
}