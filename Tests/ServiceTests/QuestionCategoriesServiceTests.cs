using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using QuestionaireApi;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Services;
using Shared.Models;

namespace Tests.ServiceTests;

public class QuestionCategoriesServiceTests
{
    private readonly Mock<QuestionaireDbContext> mockContext;
    private readonly Mock<DbSet<QuestionCategory>> mockDbSet;
    private readonly List<QuestionCategory> questionCategoriesData;
    private readonly IQuestionCategoriesService service;

    public QuestionCategoriesServiceTests()
    {
        var options = new DbContextOptionsBuilder<QuestionaireDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid()
                .ToString())
            .Options;

        mockContext = new Mock<QuestionaireDbContext>(options);
        questionCategoriesData = new List<QuestionCategory>();

        mockDbSet = questionCategoriesData.AsQueryable().BuildMockDbSet();

        mockDbSet.Setup(m => m.AddRangeAsync(It.IsAny<IEnumerable<QuestionCategory>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<QuestionCategory>, CancellationToken>((entities, ct) =>
                questionCategoriesData.AddRange(entities))
            .Returns(Task.CompletedTask);

        mockContext.Setup(c => c.QuestionCategories).Returns(mockDbSet.Object);

        service = new QuestionCategoriesService(mockContext.Object);
    }

    // --- CreateQuestionCategories Tests ---

    [Fact]
    public async Task CreateQuestionCategories_ShouldDoNothing_WhenInputDtoListIsEmpty()
    {
        // Arrange
        var questionId = 301;
        var emptyCategoriesDto = new List<CategoryExtendedDto>();
        int initialCount = questionCategoriesData.Count;

        // Act
        await service.CreateQuestionCategories(questionId, emptyCategoriesDto);

        // Assert
        mockDbSet.Verify(db => db.AddRangeAsync(
                It.IsAny<IEnumerable<QuestionCategory>>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        Assert.Equal(initialCount, questionCategoriesData.Count);
    }

    [Fact]
    public async Task CreateQuestionCategories_ShouldAddMappedCategories_ToContextDbSet()
    {
        // Arrange
        var questionId = 101;
        var categoriesDto = new List<CategoryExtendedDto>
        {
            new CategoryExtendedDto(1) { CategoryName = "Cat 1" },
            new CategoryExtendedDto(2) { CategoryName = "Cat 2" }
        };
        int initialCount = questionCategoriesData.Count;

        // Act
        await service.CreateQuestionCategories(questionId, categoriesDto);

        // Assert
        mockDbSet.Verify(db => db.AddRangeAsync(
                It.Is<IEnumerable<QuestionCategory>>(list =>
                    list.Count() == 2 &&
                    list.All(qc => qc.QuestionId == questionId) &&
                    list.Any(qc => qc.CategoryId == 1) &&
                    list.Any(qc => qc.CategoryId == 2)),
                It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.Equal(initialCount + 2, questionCategoriesData.Count);
        Assert.Contains(questionCategoriesData, qc => qc.QuestionId == questionId && qc.CategoryId == 1);
        Assert.Contains(questionCategoriesData, qc => qc.QuestionId == questionId && qc.CategoryId == 2);

        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateQuestionCategories_ShouldThrowWrappedException_WhenAddRangeAsyncFails()
    {
        // Arrange
        var questionId = 102;
        var categoriesDto = new List<CategoryExtendedDto> { new CategoryExtendedDto(1) };
        var dbException = new DbUpdateException("Simulated database error during AddRangeAsync");

        mockDbSet.Setup(db =>
                db.AddRangeAsync(It.IsAny<IEnumerable<QuestionCategory>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateQuestionCategories(questionId, categoriesDto)
        );

        Assert.Equal("An error occurred while creating  the question categories.", ex.Message);
        Assert.Equal(dbException, ex.InnerException);
    }

    // --- UpdateQuestionCategories Tests ---

    [Fact]
    public async Task UpdateQuestionCategories_ShouldAddAllDtos_WhenExistingIsEmpty()
    {
        // Arrange
        var questionId = 302;
        var existingQuestionCategories = new List<QuestionCategory>();
        var updatedCategoriesDto = new List<CategoryExtendedDto>
        {
            new CategoryExtendedDto(50) { CategoryName = "New 50" },
            new CategoryExtendedDto(60) { CategoryName = "New 60" }
        };

        // Act
        await service.UpdateQuestionCategories(questionId, existingQuestionCategories, updatedCategoriesDto);

        // Assert
        Assert.Equal(2, existingQuestionCategories.Count);
        Assert.Contains(existingQuestionCategories, qc => qc.CategoryId == 50 && qc.QuestionId == questionId);
        Assert.Contains(existingQuestionCategories, qc => qc.CategoryId == 60 && qc.QuestionId == questionId);
    }

    [Fact]
    public async Task UpdateQuestionCategories_ShouldRemoveAllExisting_WhenUpdatedDtosIsEmpty()
    {
        // Arrange
        var questionId = 303;
        var existingQuestionCategories = new List<QuestionCategory>
        {
            new QuestionCategory { QuestionId = questionId, CategoryId = 70 },
            new QuestionCategory { QuestionId = questionId, CategoryId = 80 }
        };
        var emptyUpdatedCategoriesDto = new List<CategoryExtendedDto>();

        // Act
        await service.UpdateQuestionCategories(questionId, existingQuestionCategories, emptyUpdatedCategoriesDto);

        // Assert
        Assert.Empty(existingQuestionCategories);
    }

    [Fact]
    public async Task UpdateQuestionCategories_ShouldRemoveItems_NotInUpdatedList()
    {
        // Arrange
        var questionId = 201;
        var existingQuestionCategories = new List<QuestionCategory>
        {
            new QuestionCategory { QuestionId = questionId, CategoryId = 10 },
            new QuestionCategory { QuestionId = questionId, CategoryId = 20 }
        };
        var updatedCategoriesDto = new List<CategoryExtendedDto>
        {
            new CategoryExtendedDto(10) { CategoryName = "Keep Cat" }
        };

        // Act
        await service.UpdateQuestionCategories(questionId, existingQuestionCategories, updatedCategoriesDto);

        // Assert
        Assert.Single(existingQuestionCategories);
        Assert.Contains(existingQuestionCategories, qc => qc.CategoryId == 10);
        Assert.DoesNotContain(existingQuestionCategories, qc => qc.CategoryId == 20);
    }

    [Fact]
    public async Task UpdateQuestionCategories_ShouldAddNewItems_FromUpdatedList()
    {
        // Arrange
        var questionId = 202;
        var existingQuestionCategories = new List<QuestionCategory>
        {
            new QuestionCategory { QuestionId = questionId, CategoryId = 10 }
        };
        var updatedCategoriesDto = new List<CategoryExtendedDto>
        {
            new CategoryExtendedDto(10) { CategoryName = "Existing Cat" },
            new CategoryExtendedDto(30) { CategoryName = "New Cat" }
        };

        // Act
        await service.UpdateQuestionCategories(questionId, existingQuestionCategories, updatedCategoriesDto);

        // Assert
        Assert.Equal(2, existingQuestionCategories.Count);
        Assert.Contains(existingQuestionCategories, qc => qc.CategoryId == 10);
        Assert.Contains(existingQuestionCategories,
            qc => qc.CategoryId == 30 && qc.QuestionId == questionId);
    }

    [Fact]
    public async Task UpdateQuestionCategories_ShouldUpdateProperties_OfExistingItems()
    {
        // Arrange
        var questionId = 203;
        var differentQuestionId = 999;
        var existingQuestionCategories = new List<QuestionCategory>
        {
            new QuestionCategory { QuestionId = differentQuestionId, CategoryId = 10 }
        };
        var updatedCategoriesDto = new List<CategoryExtendedDto>
        {
            new CategoryExtendedDto(10) { CategoryName = "Cat 10" }
        };

        // Act
        await service.UpdateQuestionCategories(questionId, existingQuestionCategories, updatedCategoriesDto);

        // Assert
        Assert.Single(existingQuestionCategories);
        var updatedItem = existingQuestionCategories.First();
        Assert.Equal(questionId, updatedItem.QuestionId);
        Assert.Equal(10, updatedItem.CategoryId);
    }

    [Fact]
    public async Task UpdateQuestionCategories_ShouldHandleMixedAddRemove_Correctly()
    {
        // Arrange
        var questionId = 204;
        var existingQuestionCategories = new List<QuestionCategory>
        {
            new QuestionCategory { QuestionId = questionId, CategoryId = 10 },
            new QuestionCategory { QuestionId = questionId, CategoryId = 20 },
            new QuestionCategory { QuestionId = questionId, CategoryId = 30 }
        };
        var updatedCategoriesDto = new List<CategoryExtendedDto>
        {
            new CategoryExtendedDto(10) { CategoryName = "Cat 10" },
            new CategoryExtendedDto(30) { CategoryName = "Cat 30" },
            new CategoryExtendedDto(40) { CategoryName = "Cat 40" }
        };

        // Act
        await service.UpdateQuestionCategories(questionId, existingQuestionCategories, updatedCategoriesDto);

        // Assert
        Assert.Equal(3, existingQuestionCategories.Count);
        Assert.Contains(existingQuestionCategories, qc => qc.CategoryId == 10);
        Assert.Contains(existingQuestionCategories, qc => qc.CategoryId == 30);
        Assert.Contains(existingQuestionCategories, qc => qc.CategoryId == 40 && qc.QuestionId == questionId);
        Assert.DoesNotContain(existingQuestionCategories, qc => qc.CategoryId == 20);
    }

    [Fact]
    public async Task UpdateQuestionCategories_ShouldReturnCompletedTask_OnSuccess()
    {
        // Arrange
        var questionId = 205;
        var existingQuestionCategories = new List<QuestionCategory>();
        var updatedCategoriesDto = new List<CategoryExtendedDto>();

        // Act
        var task = service.UpdateQuestionCategories(questionId, existingQuestionCategories, updatedCategoriesDto);
        await task;

        // Assert
        Assert.True(task.IsCompletedSuccessfully);
        Assert.False(task.IsFaulted);
        Assert.False(task.IsCanceled);
    }

    [Fact]
    public async Task UpdateQuestionCategories_ShouldReturnFaultedTask_WhenAddingToCollectionThrowsException()
    {
        // Arrange
        var questionId = 206;
        var simulatedException = new InvalidOperationException("Simulated collection error during Add!");

        var mockQuestionCategoriesCollection = new Mock<ICollection<QuestionCategory>>();
        var updatedDtos = new List<CategoryExtendedDto>
        {
            new CategoryExtendedDto(50) { CategoryName = "New Causing Error" }
        };

        mockQuestionCategoriesCollection.Setup(c => c.Add(It.IsAny<QuestionCategory>())).Throws(simulatedException);

        mockQuestionCategoriesCollection.As<IEnumerable<QuestionCategory>>()
            .Setup(m => m.GetEnumerator())
            .Returns(new List<QuestionCategory>().GetEnumerator());

        // Act
        var task = service.UpdateQuestionCategories(questionId, mockQuestionCategoriesCollection.Object, updatedDtos);

        // Assert
        var outerException = await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);

        Assert.NotNull(outerException);
        Assert.Equal("An error occurred while updating the question categories.", outerException.Message);
        Assert.NotNull(outerException.InnerException);

        Assert.Equal(simulatedException, outerException.InnerException);

        mockQuestionCategoriesCollection.Verify(c => c.Add(It.IsAny<QuestionCategory>()), Times.Once);
    }
}