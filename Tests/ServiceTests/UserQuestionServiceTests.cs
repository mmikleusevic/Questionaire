using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using QuestionaireApi;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Services;

namespace Tests.ServiceTests;

public class UserQuestionHistoryServiceTests
{
    private readonly Mock<QuestionaireDbContext> mockContext;
    private readonly Mock<DbSet<UserQuestionHistory>> mockDbSet;
    private readonly List<UserQuestionHistory> userQuestionHistoryData;
    private readonly IUserQuestionHistoryService userQuestionService;

    public UserQuestionHistoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<QuestionaireDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid()
                .ToString())
            .Options;

        mockContext = new Mock<QuestionaireDbContext>(options);
        userQuestionHistoryData = new List<UserQuestionHistory>();

        mockDbSet = userQuestionHistoryData.AsQueryable().BuildMockDbSet();

        mockDbSet.Setup(m =>
                m.AddRangeAsync(It.IsAny<IEnumerable<UserQuestionHistory>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<UserQuestionHistory>, CancellationToken>((entities, ct) =>
                userQuestionHistoryData.AddRange(entities))
            .Returns(Task.CompletedTask);

        mockContext.Setup(c => c.UserQuestionHistory).Returns(mockDbSet.Object);

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        userQuestionService = new UserQuestionHistoryService(mockContext.Object);
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
            new UserQuestionHistory { UserId = userIdToReset, QuestionId = 1, RoundNumber = 1 },
            new UserQuestionHistory { UserId = userIdToReset, QuestionId = 2, RoundNumber = 1 },
            new UserQuestionHistory { UserId = userIdToKeep, QuestionId = 3, RoundNumber = 1 }
        });

        // Act
        await Assert.ThrowsAsync<NotSupportedException>(() =>
            userQuestionService.ResetUserQuestionHistory(userIdToReset));
    }

    [Fact(Skip = "ExecuteDeleteAsync failure simulation is not possible with the InMemory database provider.")]
    public async Task ResetUserQuestionHistory_ThrowsWrappedException_WhenDbOperationFails()
    {
        // Arrange
        var userId = "user-to-reset-fail";


        // Act & Assert
        await Assert.ThrowsAsync<NotSupportedException>(
            () => userQuestionService.ResetUserQuestionHistory(userId)
        );
    }

    // --- CreateUserQuestionHistory Tests ---

    [Fact]
    public async Task CreateUserQuestionHistory_DoesNothing_WhenQuestionsListIsEmpty()
    {
        // Arrange
        var userId = "user-empty-history";
        var emptyQuestionsIds = new List<int>();
        int initialCount = userQuestionHistoryData.Count;

        // Act
        await userQuestionService.CreateUserQuestionHistory(userId, emptyQuestionsIds);

        // Assert
        mockDbSet.Verify(db => db.AddRangeAsync(
                It.Is<IEnumerable<UserQuestionHistory>>(list => !list.Any()),
                It.IsAny<CancellationToken>()),
            Times.Once);

        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        Assert.Equal(initialCount, userQuestionHistoryData.Count);
    }

    [Fact]
    public async Task CreateUserQuestionHistory_AddsMappedEntriesAndSavesChanges()
    {
        // Arrange
        var userId = "user-creating-history";
        var questionIds = new List<int>
        {
            10, 20
        };
        int initialCount = userQuestionHistoryData.Count;

        // Act
        await userQuestionService.CreateUserQuestionHistory(userId, questionIds);

        // Assert
        mockDbSet.Verify(db => db.AddRangeAsync(
                It.Is<IEnumerable<UserQuestionHistory>>(list =>
                    list.Count() == 2 &&
                    list.All(h => h.UserId == userId && h.RoundNumber == 1) &&
                    list.Any(h => h.QuestionId == 10) &&
                    list.Any(h => h.QuestionId == 20)),
                It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.Equal(initialCount + 2, userQuestionHistoryData.Count);
        Assert.Contains(userQuestionHistoryData, h => h.UserId == userId && h.QuestionId == 10 && h.RoundNumber == 1);
        Assert.Contains(userQuestionHistoryData, h => h.UserId == userId && h.QuestionId == 20 && h.RoundNumber == 1);

        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserQuestionHistory_ThrowsWrappedException_WhenAddRangeAsyncFails()
    {
        // Arrange
        var userId = "user-creating-history-fail1";
        var questionIds = new List<int> { 30 };
        var dbException = new InvalidOperationException("Simulated AddRangeAsync failure");

        mockDbSet.Setup(db =>
                db.AddRangeAsync(It.IsAny<IEnumerable<UserQuestionHistory>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => userQuestionService.CreateUserQuestionHistory(userId, questionIds)
        );

        Assert.Equal($"An error occurred while creating question history for user with ID {userId}.", ex.Message);
        Assert.Equal(dbException, ex.InnerException);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
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
            () => userQuestionService.CreateUserQuestionHistory(userId, questionIds)
        );

        Assert.Equal($"An error occurred while creating question history for user with ID {userId}.", ex.Message);
        Assert.Equal(dbException, ex.InnerException);
        mockDbSet.Verify(
            db => db.AddRangeAsync(It.IsAny<IEnumerable<UserQuestionHistory>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}