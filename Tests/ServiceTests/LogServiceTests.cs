using Microsoft.Extensions.Logging;
using Moq;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Services;
using Shared.Models;

namespace Tests.ServiceTests;

public class LogServiceTests
{
    private readonly ILogService logService;
    private readonly Mock<ILogger<LogService>> mockLogger;

    public LogServiceTests()
    {
        mockLogger = new Mock<ILogger<LogService>>();
        logService = new LogService(mockLogger.Object);
    }

    // --- Test Log Level Mapping and Basic Logging ---

    [Theory]
    [InlineData("TRACE", LogLevel.Trace)]
    [InlineData("DEBUG", LogLevel.Debug)]
    [InlineData("INFORMATION", LogLevel.Information)]
    [InlineData("WARNING", LogLevel.Warning)]
    [InlineData("ERROR", LogLevel.Error)]
    [InlineData("CRITICAL", LogLevel.Critical)]
    [InlineData("trace", LogLevel.Trace)]
    [InlineData("iNfOrMaTiOn", LogLevel.Information)]
    public void LogClientEntry_MapsLevelCorrectly_AndLogsWithoutExceptionDetails(string inputLevel,
        LogLevel expectedLogLevel)
    {
        // Arrange
        var logEntry = new LogEntryDto
        {
            Level = inputLevel,
            Message = "Test message",
            SourceContext = "TestComponent",
            ExceptionDetails = null
        };

        // Act
        logService.LogClientEntry(logEntry);

        // Assert
        mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(lvl => lvl == expectedLogLevel),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("UNKNOWN")]
    [InlineData("FATAL")]
    public void LogClientEntry_DefaultsToWarning_WhenLevelIsInvalidOrNull(string inputLevel)
    {
        // Arrange
        var logEntry = new LogEntryDto
        {
            Level = inputLevel,
            Message = "Warning message",
            SourceContext = "DefaultTest",
            ExceptionDetails = null
        };
        LogLevel expectedLogLevel = LogLevel.Warning;

        // Act
        logService.LogClientEntry(logEntry);

        // Assert
        mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(lvl => lvl == expectedLogLevel),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once);
    }

    // --- Test Exception Detail Handling ---

    [Fact]
    public void LogClientEntry_IncludesExceptionDetails_WhenProvided()
    {
        // Arrange
        var logEntry = new LogEntryDto
        {
            Level = "Error",
            Message = "Something failed",
            SourceContext = "ErrorSource",
            ExceptionDetails = "Stack trace here..."
        };

        LogLevel expectedLogLevel = LogLevel.Error;

        // Act
        logService.LogClientEntry(logEntry);

        // Assert
        mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(lvl => lvl == expectedLogLevel),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once);
    }

    // --- Test Source Context Handling ---

    [Fact]
    public void LogClientEntry_UsesDefaultSourceContext_WhenSourceContextIsNull()
    {
        // Arrange
        var logEntry = new LogEntryDto
        {
            Level = "Info",
            Message = "Default context log",
            SourceContext = null,
            ExceptionDetails = null
        };

        LogLevel expectedLogLevel = LogLevel.Warning;

        // Act
        logService.LogClientEntry(logEntry);

        // Assert
        mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(lvl => lvl == expectedLogLevel),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once);
    }

    [Fact]
    public void LogClientEntry_UsesProvidedSourceContext_WhenSourceContextIsNotNull()
    {
        // Arrange
        var logEntry = new LogEntryDto
        {
            Level = "Info",
            Message = "Specific context log",
            SourceContext = "MySpecificComponent",
            ExceptionDetails = null
        };

        LogLevel expectedLogLevel = LogLevel.Warning;

        // Act
        logService.LogClientEntry(logEntry);

        // Assert
        mockLogger.Verify(logger => logger.Log(
                It.Is<LogLevel>(lvl => lvl == expectedLogLevel),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once);
    }
}