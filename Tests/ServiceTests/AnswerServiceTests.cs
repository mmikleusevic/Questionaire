using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using QuestionaireApi;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Services;
using Shared.Models;

namespace Tests.ServiceTests;

public class AnswerServiceTests
{
    private readonly List<Answer> answersData;
    private readonly IAnswerService answerService;
    private readonly Mock<QuestionaireDbContext> mockContext;
    private readonly Mock<DbSet<Answer>> mockDbSet;

    public AnswerServiceTests()
    {
        var options = new DbContextOptionsBuilder<QuestionaireDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        mockContext = new Mock<QuestionaireDbContext>(options);
        answersData = new List<Answer>();
        mockDbSet = answersData.AsQueryable().BuildMockDbSet();

        mockDbSet.Setup(m => m.AddRangeAsync(It.IsAny<IEnumerable<Answer>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Answer>,
                CancellationToken>((entities, ct) => answersData.AddRange(entities))
            .Returns(Task.CompletedTask);

        mockContext.Setup(c => c.Answers).Returns(mockDbSet.Object);

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        answerService = new AnswerService(mockContext.Object);
    }

    // ================== CreateQuestionAnswers Tests ==================

    [Fact]
    public async Task CreateQuestionAnswers_ShouldAddMappedAnswers_ToContext()
    {
        // Arrange
        var questionId = 1;
        var inputDtos = new List<AnswerExtendedDto>
        {
            new AnswerExtendedDto { AnswerText = "Answer 1", IsCorrect = true },
            new AnswerExtendedDto { AnswerText = "Answer 2", IsCorrect = false }
        };

        // Act
        await answerService.CreateQuestionAnswers(questionId, inputDtos);

        // Assert

        mockDbSet.Verify(db => db.AddRangeAsync(
                It.Is<IEnumerable<Answer>>(list => list.Count() == 2),
                It.IsAny<CancellationToken>()),
            Times.Once);

        Assert.Equal(2, answersData.Count);
        Assert.Contains(answersData, a => a.AnswerText == "Answer 1" && a.IsCorrect && a.QuestionId == questionId);
        Assert.Contains(answersData, a => a.AnswerText == "Answer 2" && !a.IsCorrect && a.QuestionId == questionId);
    }

    [Fact]
    public async Task CreateQuestionAnswers_ShouldThrowInvalidOperationException_WhenAddRangeAsyncFails()
    {
        // Arrange
        var questionId = 1;
        var inputDtos = new List<AnswerExtendedDto> { new AnswerExtendedDto { AnswerText = "Test" } };
        var dbException = new DbUpdateException("Database error simulation", new Exception("Inner"));

        mockDbSet.Setup(db => db.AddRangeAsync(It.IsAny<IEnumerable<Answer>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => answerService.CreateQuestionAnswers(questionId, inputDtos)
        );

        Assert.Equal("An error occurred while updating the question answers.", ex.Message);
        Assert.Equal(dbException, ex.InnerException);
    }


    // ================== UpdateQuestionAnswers Tests ==================

    [Fact]
    public async Task UpdateQuestionAnswers_ShouldRemoveAnswers_NotInUpdatedList()
    {
        // Arrange
        var questionId = 10;
        var existingAnswers = new List<Answer>
        {
            new Answer { Id = 1, QuestionId = questionId, AnswerText = "Keep Me", IsCorrect = true },
            new Answer { Id = 2, QuestionId = questionId, AnswerText = "Delete Me", IsCorrect = false }
        };
        var updatedDtos = new List<AnswerExtendedDto>
        {
            new AnswerExtendedDto(1) { AnswerText = "Keep Me Updated", IsCorrect = true }
        };

        // Act
        await answerService.UpdateQuestionAnswers(questionId, existingAnswers, updatedDtos);

        // Assert
        Assert.Single(existingAnswers);
        Assert.DoesNotContain(existingAnswers, a => a.Id == 2);
        Assert.Contains(existingAnswers, a => a.Id == 1);
    }

    [Fact]
    public async Task UpdateQuestionAnswers_ShouldUpdateExistingAnswers_WithMatchingIds()
    {
        // Arrange
        var questionId = 10;
        var existingAnswers = new List<Answer>
        {
            new Answer { Id = 1, QuestionId = questionId, AnswerText = "Old Text", IsCorrect = false },
            new Answer { Id = 2, QuestionId = questionId, AnswerText = "Another Answer", IsCorrect = true }
        };
        var updatedDtos = new List<AnswerExtendedDto>
        {
            new AnswerExtendedDto(1) { AnswerText = "New Text", IsCorrect = true },
            new AnswerExtendedDto(2) { AnswerText = "Another Answer", IsCorrect = true }
        };

        // Act
        await answerService.UpdateQuestionAnswers(questionId, existingAnswers, updatedDtos);

        // Assert
        Assert.Equal(2, existingAnswers.Count);
        var updatedAnswer1 = existingAnswers.FirstOrDefault(a => a.Id == 1);
        Assert.NotNull(updatedAnswer1);
        Assert.Equal("New Text", updatedAnswer1.AnswerText);
        Assert.True(updatedAnswer1.IsCorrect);

        var updatedAnswer2 = existingAnswers.FirstOrDefault(a => a.Id == 2);
        Assert.NotNull(updatedAnswer2);
        Assert.Equal("Another Answer", updatedAnswer2.AnswerText);
        Assert.True(updatedAnswer2.IsCorrect);
    }

    [Fact]
    public async Task UpdateQuestionAnswers_ShouldAddNewAnswers_FromUpdatedList()
    {
        // Arrange
        var questionId = 10;
        var existingAnswers = new List<Answer>
        {
            new Answer { Id = 1, QuestionId = questionId, AnswerText = "Existing", IsCorrect = true }
        };
        var updatedDtos = new List<AnswerExtendedDto>
        {
            new AnswerExtendedDto(1) { AnswerText = "Existing", IsCorrect = true },
            new AnswerExtendedDto(0) { AnswerText = "New Answer 1", IsCorrect = false },
            new AnswerExtendedDto { AnswerText = "New Answer 2", IsCorrect = true }
        };

        // Act
        await answerService.UpdateQuestionAnswers(questionId, existingAnswers, updatedDtos);

        // Assert
        Assert.Equal(3, existingAnswers.Count);
        Assert.Contains(existingAnswers, a => a.Id == 1);
        Assert.Contains(existingAnswers,
            a => a.AnswerText == "New Answer 1" && !a.IsCorrect && a.QuestionId == questionId && a.Id == 0);
        Assert.Contains(existingAnswers,
            a => a.AnswerText == "New Answer 2" && a.IsCorrect && a.QuestionId == questionId && a.Id == 0);
    }

    [Fact]
    public async Task UpdateQuestionAnswers_ShouldHandleMixedAddUpdateRemove_Correctly()
    {
        // Arrange
        var questionId = 10;
        var existingAnswers = new List<Answer>
        {
            new Answer { Id = 1, QuestionId = questionId, AnswerText = "To Update", IsCorrect = false },
            new Answer { Id = 2, QuestionId = questionId, AnswerText = "To Remove", IsCorrect = true },
            new Answer { Id = 3, QuestionId = questionId, AnswerText = "To Keep", IsCorrect = false }
        };
        var updatedDtos = new List<AnswerExtendedDto>
        {
            new AnswerExtendedDto(1) { AnswerText = "Updated Text", IsCorrect = true },
            new AnswerExtendedDto(3) { AnswerText = "To Keep", IsCorrect = false },
            new AnswerExtendedDto(0) { AnswerText = "Newly Added", IsCorrect = true }
        };

        // Act
        await answerService.UpdateQuestionAnswers(questionId, existingAnswers, updatedDtos);

        // Assert
        Assert.Equal(3, existingAnswers.Count);

        Assert.DoesNotContain(existingAnswers, a => a.Id == 2);

        var updated = existingAnswers.FirstOrDefault(a => a.Id == 1);
        Assert.NotNull(updated);
        Assert.Equal("Updated Text", updated.AnswerText);
        Assert.True(updated.IsCorrect);

        Assert.Contains(existingAnswers, a => a.Id == 3 && a.AnswerText == "To Keep" && !a.IsCorrect);
        Assert.Contains(existingAnswers,
            a => a.Id == 0 && a.AnswerText == "Newly Added" && a.IsCorrect && a.QuestionId == questionId);
    }


    [Fact]
    public async Task UpdateQuestionAnswers_ShouldReturnCompletedTask_OnSuccess()
    {
        // Arrange
        var questionId = 1;
        var existingAnswers = new List<Answer>();
        var updatedDtos = new List<AnswerExtendedDto>();

        // Act
        var task = answerService.UpdateQuestionAnswers(questionId, existingAnswers, updatedDtos);
        await task;

        // Assert
        Assert.True(task.IsCompletedSuccessfully);
    }

    [Fact]
    public async Task UpdateQuestionAnswers_ShouldReturnFaultedTask_WhenLogicThrowsException()
    {
        // Arrange
        var questionId = 10;
        var simulatedException = new InvalidOperationException("Simulated LINQ error!");

        var existingAnswers = new List<Answer>
        {
            new Answer { Id = 1, QuestionId = questionId, AnswerText = "Keep Me" }
        };

        var mockUpdatedDtos = new Mock<List<AnswerExtendedDto>>();

        var mockAnswersCollection = new Mock<ICollection<Answer>>();
        var updatedDtos = new List<AnswerExtendedDto>
        {
            new AnswerExtendedDto(0) { AnswerText = "New Answer", IsCorrect = true }
        };

        mockAnswersCollection.Setup(c => c.Add(It.IsAny<Answer>())).Throws(simulatedException);
        mockAnswersCollection.As<IEnumerable<Answer>>().Setup(m => m.GetEnumerator())
            .Returns(new List<Answer>().GetEnumerator());

        // Act
        var task = answerService.UpdateQuestionAnswers(questionId, mockAnswersCollection.Object, updatedDtos);

        // Assert
        var outerException = await Assert.ThrowsAsync<InvalidOperationException>(async () => await task);

        Assert.NotNull(outerException);
        Assert.Equal("An error occurred while updating the question answers.", outerException.Message);
        Assert.NotNull(outerException.InnerException);
        Assert.Equal(simulatedException, outerException.InnerException);

        mockAnswersCollection.Verify(c => c.Add(It.IsAny<Answer>()), Times.Once);
    }
}