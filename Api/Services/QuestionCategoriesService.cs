using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using Shared.Models;

namespace QuestionaireApi.Services;

public class QuestionCategoriesService(QuestionaireDbContext context) : IQuestionCategoriesService
{
    public async Task CreateQuestionCategories(int questionId, List<CategoryExtendedDto> categories)
    {
        try
        {
            await context.QuestionCategories.AddRangeAsync(
                categories.Select(a => new QuestionCategory
                {
                    QuestionId = questionId,
                    CategoryId = a.Id
                }).ToList()
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while creating  the question categories.", ex);
        }
    }

    public Task UpdateQuestionCategories(int questionId, ICollection<QuestionCategory> questionCategories,
        List<CategoryExtendedDto> categories)
    {
        try
        {
            List<QuestionCategory> questionCategoriesToRemove = questionCategories
                .Where(pa => categories.All(ua => ua.Id != pa.CategoryId))
                .ToList();

            foreach (QuestionCategory questionCategory in questionCategoriesToRemove)
            {
                questionCategories.Remove(questionCategory);
            }

            foreach (CategoryExtendedDto updatedCategory in categories)
            {
                QuestionCategory? existingQuestionCategory = questionCategories
                    .FirstOrDefault(pa => pa.CategoryId == updatedCategory.Id);

                if (existingQuestionCategory != null)
                {
                    existingQuestionCategory.QuestionId = questionId;
                    existingQuestionCategory.CategoryId = updatedCategory.Id;
                }
                else
                {
                    questionCategories.Add(new QuestionCategory
                    {
                        QuestionId = questionId,
                        CategoryId = updatedCategory.Id
                    });
                }
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(
                new InvalidOperationException("An error occurred while updating the question categories.", ex));
        }
    }
}