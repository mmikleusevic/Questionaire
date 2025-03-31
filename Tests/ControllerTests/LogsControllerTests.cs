using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QuestionaireApi.Controllers;
using QuestionaireApi.Interfaces;
using Shared.Models;

namespace Tests.ControllerTests;

public class LogsControllerTests
{
    private readonly LogsController controller;
    private readonly Mock<ILogger<LogsController>> mockLogger;
    private readonly Mock<ILogService> mockLogService;

    public LogsControllerTests()
    {
        mockLogService = new Mock<ILogService>();
        mockLogger = new Mock<ILogger<LogsController>>();

        controller = new LogsController(
            mockLogger.Object,
            mockLogService.Object);
    }

    private void VerifyLogError<T>(Mock<ILogger<T>> loggerMock, Exception expectedException,
        string expectedMessageContains)
    {
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(expectedMessageContains)),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }

    // --- PostLogEntry Tests ---

    [Fact]
    public void PostLogEntry_ReturnsOkResult_WhenLogEntryIsValid()
    {
        // Arrange
        var validLogEntry = new LogEntryDto { Message = "Client started", Level = "Info" };
        mockLogService.Setup(s => s.LogClientEntry(validLogEntry));

        // Act
        var result = controller.PostLogEntry(validLogEntry);

        // Assert
        Assert.IsType<OkResult>(result);
        mockLogService.Verify(s => s.LogClientEntry(validLogEntry), Times.Once);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);
    }

    [Fact]
    public void PostLogEntry_ReturnsBadRequest_WhenLogEntryIsNull()
    {
        // Arrange
        LogEntryDto? logEntry = null;

        // Act
        var result = controller.PostLogEntry(logEntry);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid log entry format.", badRequestResult.Value);
        mockLogService.Verify(s => s.LogClientEntry(It.IsAny<LogEntryDto>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void PostLogEntry_ReturnsBadRequest_WhenLogEntryMessageIsNullOrWhitespace(string invalidMessage)
    {
        // Arrange
        var logEntry = new LogEntryDto { Message = invalidMessage, Level = "Warn" };

        // Act
        var result = controller.PostLogEntry(logEntry);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid log entry format.", badRequestResult.Value);
        mockLogService.Verify(s => s.LogClientEntry(It.IsAny<LogEntryDto>()), Times.Never);
    }

    [Fact]
    public void PostLogEntry_ReturnsStatusCode500_WhenLogServiceThrowsException()
    {
        // Arrange
        var validLogEntry = new LogEntryDto { Message = "Important client event", Level = "Error" };
        var serviceException = new InvalidOperationException("Failed to write to log sink.");
        mockLogService.Setup(s => s.LogClientEntry(validLogEntry)).Throws(serviceException);

        string expectedLogMessage = "Error occurred while processing client log entry via LogService.";
        string expectedResponseMessage = "An error occurred while processing the log entry.";

        // Act
        var result = controller.PostLogEntry(validLogEntry);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal(expectedResponseMessage, objectResult.Value);
        mockLogService.Verify(s => s.LogClientEntry(validLogEntry), Times.Once);
        VerifyLogError(mockLogger, serviceException, expectedLogMessage);
    }
}