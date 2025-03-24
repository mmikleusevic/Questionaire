using QuestionaireApi.Models.Database;
using Shared.Models;

namespace QuestionaireApi.Interfaces;

public interface IQuestionCategoriesService
{
    Task CreateQuestionCategories(int questionId, List<CategoryExtendedDto> categories);

    Task UpdateQuestionCategories(int questionId, ICollection<QuestionCategory> questionCategories,
        List<CategoryExtendedDto> categories);
}