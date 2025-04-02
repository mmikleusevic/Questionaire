using Shared.Models;
using SharedStandard.Models;

namespace Web.Interfaces;

public interface IQuestionService
{
    Task<PaginatedResponse<QuestionExtendedDto>> GetQuestions(QuestionsRequestDto questionsRequest);
    Task<List<QuestionExtendedDto>> GetRandomUniqueQuestions(UniqueQuestionsRequestDto uniqueQuestionsRequestDto);
    Task ApproveQuestion(int id);
    Task CreateQuestion(QuestionExtendedDto newQuestion);
    Task UpdateQuestion(QuestionExtendedDto updatedQuestion);
    Task DeleteQuestion(int id);
}