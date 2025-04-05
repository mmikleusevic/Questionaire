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

    // Helper method to capture and verify log messages

    private void VerifyLog(
        LogLevel expectedLevel,
        string expectedSourceContext,
        string expectedMessage,
        string? expectedExceptionDetails,
        Times times)
    {
        mockLogger.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(lvl => lvl == expectedLevel),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, type) =>
                    ValidateLogState(state, expectedSourceContext, expectedMessage, expectedExceptionDetails)
                ),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            times);
    }

    // Helper to invoke the formatter and check its output

    private bool ValidateLogState(object state, string expectedSourceContext, string expectedMessage,
        string? expectedExceptionDetails)
    {
        var logValues = state as IReadOnlyList<KeyValuePair<string, object?>>;
        if (logValues == null)
        {
            Console.WriteLine("Warning: Log state object was not the expected list type.");
            return false;
        }

        bool contextMatch = logValues.Any(kv =>
            kv.Key == "ClientSourceContext" && kv.Value?.ToString() == expectedSourceContext);
        bool messageMatch = logValues.Any(kv => kv.Key == "ClientMessage" && kv.Value?.ToString() == expectedMessage);
        bool exceptionMatch = true;

        if (expectedExceptionDetails != null)
        {
            exceptionMatch = logValues.Any(kv =>
                kv.Key == "ClientException" && kv.Value?.ToString() == expectedExceptionDetails);
        }
        else
        {
            exceptionMatch = logValues.All(kv => kv.Key != "ClientException");
        }

        bool formatMatch = logValues.Any(kv => kv.Key == "{OriginalFormat}");

        return contextMatch && messageMatch && exceptionMatch && formatMatch;
    }

    // --- Test Log Level Mapping and Basic Logging ---

    [Theory]
    [InlineData("TRACE", LogLevel.Trace, "Trace message")]
    [InlineData("DEBUG", LogLevel.Debug, "Debug message")]
    [InlineData("INFORMATION", LogLevel.Information, "Info message")]
    [InlineData("WARNING", LogLevel.Warning, "Warn message")]
    [InlineData("ERROR", LogLevel.Error, "Error message")]
    [InlineData("CRITICAL", LogLevel.Critical, "Critical message")]
    [InlineData("trace", LogLevel.Trace, "Lowercase trace")]
    [InlineData("iNfOrMaTiOn", LogLevel.Information, "Mixed case info")]
    public void LogClientEntry_MapsLevelCorrectly_AndLogsMessageContent(string inputLevel, LogLevel expectedLogLevel,
        string message)
    {
        // Arrange
        var sourceContext = "TestComponent";
        var logEntry = new LogEntryDto
        {
            Level = inputLevel,
            Message = message,
            SourceContext = "TestComponent",
            ExceptionDetails = null
        };

        // Act
        logService.LogClientEntry(logEntry);

        // Assert
        VerifyLog(expectedLogLevel, sourceContext, message, null, Times.Once());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("UNKNOWN")]
    [InlineData("FATAL")]
    public void LogClientEntry_DefaultsToWarning_WhenLevelIsInvalidOrNull(string inputLevel)
    {
        // Arrange
        var sourceContext = "DefaultTest";
        var message = "Default level message";
        var logEntry = new LogEntryDto
        {
            Level = inputLevel,
            Message = message,
            SourceContext = sourceContext,
            ExceptionDetails = null
        };
        LogLevel expectedLogLevel = LogLevel.Warning;

        // Act
        logService.LogClientEntry(logEntry);

        // Assert
        VerifyLog(expectedLogLevel, sourceContext, message, null, Times.Once());
    }

    // --- Test Exception Detail Handling ---

    [Fact]
    public void LogClientEntry_IncludesExceptionDetailsInLogMessage_WhenProvided()
    {
        // Arrange
        var sourceContext = "ErrorSource";
        var message = "Something failed";
        var exceptionDetails = "Stack trace here...";
        var logEntry = new LogEntryDto
        {
            Level = "Error",
            Message = message,
            SourceContext = sourceContext,
            ExceptionDetails = exceptionDetails
        };
        LogLevel expectedLogLevel = LogLevel.Error;

        // Act
        logService.LogClientEntry(logEntry);

        // Assert
        VerifyLog(expectedLogLevel, sourceContext, message, exceptionDetails, Times.Once());
    }

    // --- Test Source Context Handling ---

    [Fact]
    public void LogClientEntry_UsesDefaultSourceContext_WhenSourceContextIsNull()
    {
        // Arrange
        var message = "Default context log";
        var logEntry = new LogEntryDto
        {
            Level = "Information",
            Message = message,
            SourceContext = null,
            ExceptionDetails = null
        };
        LogLevel expectedLogLevel = LogLevel.Information;
        string expectedSourceContext = "WASM";

        // Act
        logService.LogClientEntry(logEntry);

        // Assert
        VerifyLog(expectedLogLevel, expectedSourceContext, message, null, Times.Once());
    }

    [Fact]
    public void LogClientEntry_UsesProvidedSourceContext_WhenSourceContextIsNotNull()
    {
        // Arrange
        var sourceContext = "MySpecificComponent";
        var message = "Specific context log";
        var logEntry = new LogEntryDto
        {
            Level = "Debug",
            Message = message,
            SourceContext = sourceContext,
            ExceptionDetails = null
        };
        LogLevel expectedLogLevel = LogLevel.Debug;

        // Act
        logService.LogClientEntry(logEntry);

        // Assert
        VerifyLog(expectedLogLevel, sourceContext, message, null, Times.Once());
    }
}