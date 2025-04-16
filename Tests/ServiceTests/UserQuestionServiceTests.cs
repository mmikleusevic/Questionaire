using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using QuestionaireApi;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Services;
using SharedStandard.Models;

namespace Tests.ServiceTests;

public class UserQuestionHistoryServiceTests
{
    private readonly Mock<QuestionaireDbContext> mockContext;
    private readonly Mock<DbSet<UserQuestionHistory>> mockHistoryDbSet;
    private readonly List<UserQuestionHistory> userQuestionHistoryData;
    private readonly IUserQuestionHistoryService userQuestionHistoryService;

    public UserQuestionHistoryServiceTests()
    {
        userQuestionHistoryData = new List<UserQuestionHistory>();

        var options = new DbContextOptionsBuilder<QuestionaireDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
        mockContext = new Mock<QuestionaireDbContext>(options);

        mockHistoryDbSet = userQuestionHistoryData.AsQueryable().BuildMockDbSet();

        mockContext.Setup(c => c.UserQuestionHistory).Returns(mockHistoryDbSet.Object);

        mockHistoryDbSet.Setup(m =>
                m.AddRangeAsync(It.IsAny<IEnumerable<UserQuestionHistory>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<UserQuestionHistory>, CancellationToken>((entities, ct) =>
                userQuestionHistoryData.AddRange(entities))
            .Returns(Task.CompletedTask);

        mockHistoryDbSet.Setup(m => m.AddRange(It.IsAny<IEnumerable<UserQuestionHistory>>()))
            .Callback<IEnumerable<UserQuestionHistory>>((entities) =>
                userQuestionHistoryData.AddRange(entities));


        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        userQuestionHistoryService = new UserQuestionHistoryService(mockContext.Object);
    }

    // --- ResetUserQuestionHistory Tests ---

    [Fact(Skip = "ExecuteDeleteAsync is not supported by the InMemory database provider.")]
    public async Task ResetUserQuestionHistory_AttemptsToDeleteCorrectHistory_WhenCalled()
    {
        // Arrange
        var userIdToReset = "user-to-reset";
        var userIdToKeep = "user-to-keep";

        userQuestionHistoryData.AddRange(new[]
        {
            new UserQuestionHistory { Id = 1, UserId = userIdToReset, QuestionId = 1 },
            new UserQuestionHistory { Id = 2, UserId = userIdToReset, QuestionId = 2 },
            new UserQuestionHistory { Id = 3, UserId = userIdToKeep, QuestionId = 3 }
        });

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(() =>
            userQuestionHistoryService.ResetUserQuestionHistoryForCriteria(userIdToReset, new List<int> { 1, 2 },
                new List<Difficulty>())
        );
    }

    [Fact(Skip = "ExecuteDeleteAsync failure simulation is not possible with the InMemory database provider.")]
    public async Task ResetUserQuestionHistory_ThrowsWrappedException_WhenDbOperationFails()
    {
        // Arrange
        var userId = "user-to-reset-fail";

        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(
            () => userQuestionHistoryService.ResetUserQuestionHistoryForCriteria(userId, new List<int> { 1 },
                new List<Difficulty>())
        );
    }

    // --- CreateUserQuestionHistory Tests ---

    [Fact]
    public async Task CreateUserQuestionHistory_AddsMappedEntriesAndSavesChanges()
    {
        // Arrange
        var userId = "user-creating-history";
        var questionIds = new List<int> { 10, 20 };
        int initialCount = userQuestionHistoryData.Count;
        int expectedAddedCount = 2;

        // Act
        await userQuestionHistoryService.CreateUserQuestionHistory(userId, questionIds);

        // Assert
        Assert.Equal(initialCount + expectedAddedCount, userQuestionHistoryData.Count);
        Assert.Contains(userQuestionHistoryData, h => h.UserId == userId && h.QuestionId == 10);
        Assert.Contains(userQuestionHistoryData, h => h.UserId == userId && h.QuestionId == 20);

        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserQuestionHistory_ThrowsWrappedException_WhenAddRangeAsyncFails()
    {
        // Arrange
        var userId = "user-creating-history-fail1";
        var questionIds = new List<int> { 30 };
        var dbException = new InvalidOperationException("Simulated AddRangeAsync failure");

        mockHistoryDbSet.Setup(db =>
                db.AddRangeAsync(It.IsAny<IEnumerable<UserQuestionHistory>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => userQuestionHistoryService.CreateUserQuestionHistory(userId, questionIds)
        );

        Assert.Equal($"An error occurred while creating question history for user with ID {userId}.", ex.Message);
        Assert.Equal(dbException, ex.InnerException);

        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserQuestionHistory_ThrowsWrappedException_WhenSaveChangesAsyncFails()
    {
        // Arrange
        var userId = "user-creating-history-fail2";
        var questionIds = new List<int> { 40 };
        var dbException = new DbUpdateException("Simulated SaveChanges failure");

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => userQuestionHistoryService.CreateUserQuestionHistory(userId, questionIds)
        );

        Assert.Equal($"An error occurred while creating question history for user with ID {userId}.", ex.Message);
        Assert.Equal(dbException, ex.InnerException);
        mockHistoryDbSet.Verify(
            db => db.AddRangeAsync(It.IsAny<IEnumerable<UserQuestionHistory>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}