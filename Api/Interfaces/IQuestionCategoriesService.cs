using QuestionaireApi.Models.Database;
using SharedStandard.Models;

namespace QuestionaireApi.Interfaces;

public interface IQuestionCategoriesService
{
    Task CreateQuestionCategories(int questionId, ICollection<PendingQuestionCategory> pendingQuestionCategories);

    Task UpdateQuestionCategories(int questionId, ICollection<QuestionCategory> questionCategories,
        List<CategoryDto> categories);
}