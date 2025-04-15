using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QuestionaireApi.Controllers;
using QuestionaireApi.Interfaces;
using SharedStandard.Models;

namespace Tests.ControllerTests;

public class UserQuestionHistoryControllerTests
{
    private readonly UserQuestionHistoryController controller;
    private readonly Mock<ILogger<CategoryController>> mockLogger;
    private readonly Mock<IUserQuestionHistoryService> mockUserHistoryService;

    public UserQuestionHistoryControllerTests()
    {
        mockUserHistoryService = new Mock<IUserQuestionHistoryService>();
        mockLogger = new Mock<ILogger<CategoryController>>();

        controller = new UserQuestionHistoryController(
            mockUserHistoryService.Object,
            mockLogger.Object
        );
    }

    [Fact]
    public async Task CreateUserHistory_ReturnsBadRequest_WhenInputIsNull()
    {
        // Arrange
        UserQuestionHistoryDto? userHistoryDto = null;

        // Act
        var result = await controller.CreateUserHistory(userHistoryDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("User question history data cannot be null.", badRequestResult.Value);
        mockUserHistoryService.Verify(s => s.CreateUserQuestionHistory(It.IsAny<string>(), It.IsAny<List<int>>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateUserHistory_CallsServiceAndReturnsCreated_WhenInputIsValid()
    {
        // Arrange
        var userHistoryDto = new UserQuestionHistoryDto
        {
            UserId = "test-user-1",
            QuestionIds = new List<int> { 1, 2, 3 }
        };

        mockUserHistoryService
            .Setup(s => s.CreateUserQuestionHistory(userHistoryDto.UserId, userHistoryDto.QuestionIds))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.CreateUserHistory(userHistoryDto);

        // Assert
        Assert.IsType<CreatedResult>(result);
        mockUserHistoryService.Verify(
            s => s.CreateUserQuestionHistory(userHistoryDto.UserId, userHistoryDto.QuestionIds), Times.Once);
    }

    [Fact]
    public async Task CreateUserHistory_ReturnsInternalServerErrorAndLogsError_WhenServiceThrowsException()
    {
        // Arrange
        var userHistoryDto = new UserQuestionHistoryDto
        {
            UserId = "test-user-fail",
            QuestionIds = new List<int> { 10 }
        };
        var serviceException = new InvalidOperationException("Database connection failed");
        var expectedLogMessage = "An error occurred while creating user question history.";

        mockUserHistoryService
            .Setup(s => s.CreateUserQuestionHistory(userHistoryDto.UserId, userHistoryDto.QuestionIds))
            .ThrowsAsync(serviceException);

        // Act
        var result = await controller.CreateUserHistory(userHistoryDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal(expectedLogMessage, objectResult.Value);

        mockUserHistoryService.Verify(
            s => s.CreateUserQuestionHistory(userHistoryDto.UserId, userHistoryDto.QuestionIds), Times.Once);

        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()
                        .Contains(expectedLogMessage)),
                serviceException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateUserHistory_CallsServiceAndReturnsCreated_WhenQuestionIdsIsEmpty()
    {
        // Arrange
        var userHistoryDto = new UserQuestionHistoryDto
        {
            UserId = "test-user-empty",
            QuestionIds = new List<int>()
        };

        mockUserHistoryService
            .Setup(s => s.CreateUserQuestionHistory(userHistoryDto.UserId, userHistoryDto.QuestionIds))
            .Returns(Task.CompletedTask);

        // Act
        var result = await controller.CreateUserHistory(userHistoryDto);

        // Assert
        Assert.IsType<CreatedResult>(result);
        mockUserHistoryService.Verify(
            s => s.CreateUserQuestionHistory(userHistoryDto.UserId, It.Is<List<int>>(list => !list.Any())), Times.Once);
    }
}