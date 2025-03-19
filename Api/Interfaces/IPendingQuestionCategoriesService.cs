using QuestionaireApi.Models.Database;
using Shared.Models;

namespace QuestionaireApi.Interfaces;

public interface IPendingQuestionCategoriesService
{
    Task CreatePendingQuestionCategories(int pendingQuestionId, List<CategoryExtendedDto> categories);

    Task UpdatePendingQuestionCategories(int pendingQuestionId,
        ICollection<PendingQuestionCategory> pendingQuestionCategories, List<CategoryExtendedDto> categories);
}