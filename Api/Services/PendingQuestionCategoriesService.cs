using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using Shared.Models;

namespace QuestionaireApi.Services;

public class PendingQuestionCategoriesService(QuestionaireDbContext context) : IPendingQuestionCategoriesService
{
    public async Task CreatePendingQuestionCategories(int pendingQuestionId, List<CategoryValidationDto> categories)
    {
        try
        {
            await context.PendingQuestionCategories.AddRangeAsync(
                categories.Select(a => new PendingQuestionCategory
                {
                    PendingQuestionId = pendingQuestionId,
                    CategoryId = a.Id
                }).ToList()
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while creating the pending question categories.",
                ex);
        }
    }

    public Task UpdatePendingQuestionCategories(int pendingQuestionId,
        ICollection<PendingQuestionCategory> pendingQuestionCategories, List<CategoryValidationDto> categories)
    {
        try
        {
            List<PendingQuestionCategory> pendingQuestionCategoriesToRemove = pendingQuestionCategories
                .Where(pa => categories.All(ua => ua.Id != pa.CategoryId))
                .ToList();

            foreach (PendingQuestionCategory pendingQuestionCategory in pendingQuestionCategoriesToRemove)
            {
                pendingQuestionCategories.Remove(pendingQuestionCategory);
            }

            foreach (CategoryValidationDto updatedCategory in categories)
            {
                PendingQuestionCategory? existingQuestionCategory = pendingQuestionCategories
                    .FirstOrDefault(pa => pa.CategoryId == updatedCategory.Id);

                if (existingQuestionCategory != null)
                {
                    existingQuestionCategory.PendingQuestionId = pendingQuestionId;
                    existingQuestionCategory.CategoryId = updatedCategory.Id;
                }
                else
                {
                    pendingQuestionCategories.Add(new PendingQuestionCategory
                    {
                        PendingQuestionId = pendingQuestionId,
                        CategoryId = updatedCategory.Id
                    });
                }
            }

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            return Task.FromException(
                new InvalidOperationException("An error occurred while updating the pending question categories.", ex));
        }
    }
}