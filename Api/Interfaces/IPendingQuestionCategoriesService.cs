using QuestionaireApi.Models.Database;
using Shared.Models;

namespace QuestionaireApi.Interfaces;

public interface IPendingQuestionCategoriesService
{
    Task CreatePendingQuestionCategories(int pendingQuestionId, List<CategoryValidationDto> categories);

    Task UpdatePendingQuestionCategories(int pendingQuestionId,
        ICollection<PendingQuestionCategory> pendingQuestionCategories, List<CategoryValidationDto> categories);
}