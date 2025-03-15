using QuestionaireApi.Models.Database;
using Shared.Models;

namespace QuestionaireApi.Interfaces;

public interface IQuestionCategoriesService
{
    Task CreateQuestionCategories(int questionId, ICollection<PendingQuestionCategory> pendingQuestionCategories);

    Task UpdateQuestionCategories(int questionId, ICollection<QuestionCategory> questionCategories,
        List<CategoryValidationDto> categories);
}