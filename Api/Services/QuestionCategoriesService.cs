using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Services;

public class QuestionCategoriesService(QuestionaireDbContext context) : IQuestionCategoriesService
{
    public async Task CreateQuestionCategories(int questionId,
        ICollection<PendingQuestionCategory> pendingQuestionCategories)
    {
        try
        {
            await context.QuestionCategories.AddRangeAsync(
                pendingQuestionCategories.Select(a => new QuestionCategory
                {
                    QuestionId = questionId,
                    CategoryId = a.CategoryId
                }).ToList()
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while creating  the question categories.", ex);
        }
    }

    public Task UpdateQuestionCategories(int questionId, ICollection<QuestionCategory> questionCategories,
        List<CategoryDto> categories)
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

            foreach (CategoryDto updatedCategory in categories)
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