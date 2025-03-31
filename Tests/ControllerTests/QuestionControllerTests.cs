using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using QuestionaireApi.Controllers;
using QuestionaireApi.Interfaces;
using Shared.Models;
using SharedStandard.Models;

namespace Tests.ControllerTests;

public class QuestionControllerTests
{
    private readonly QuestionController controller;
    private readonly Mock<ILogger<QuestionController>> mockLogger;
    private readonly Mock<IQuestionService> mockQuestionService;
    private readonly ClaimsPrincipal mockUser;

    public QuestionControllerTests()
    {
        mockQuestionService = new Mock<IQuestionService>();
        mockLogger = new Mock<ILogger<QuestionController>>();

        mockUser = new ClaimsPrincipal(new ClaimsIdentity([
            new Claim(ClaimTypes.Role, "User")
        ], "mock"));

        controller = new QuestionController(
            mockQuestionService.Object,
            mockLogger.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = mockUser }
        };
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

    // --- GetQuestions (Paged) Tests ---

    [Fact]
    public async Task GetQuestions_ReturnsOkObjectResult_WithPaginatedData()
    {
        // Arrange
        var requestDto = new QuestionsRequestDto { PageNumber = 1, PageSize = 50 };
        var expectedResponse = new PaginatedResponse<QuestionExtendedDto>
        {
            Items = new List<QuestionExtendedDto> { new QuestionExtendedDto(1) { QuestionText = "Q1" } },
            TotalCount = 1,
            PageSize = 50
        };
        mockQuestionService.Setup(s => s.GetQuestions(requestDto, mockUser))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await controller.GetQuestions(requestDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PaginatedResponse<QuestionExtendedDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(expectedResponse, okResult.Value);
        mockQuestionService.Verify(s => s.GetQuestions(requestDto, mockUser), Times.Once);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(0, 0)]
    public async Task GetQuestions_ReturnsBadRequest_WhenPageNumberOrSizeIsInvalid(int pageNum, int pageSize)
    {
        // Arrange
        var requestDto = new QuestionsRequestDto { PageNumber = pageNum, PageSize = pageSize };

        // Act
        var result = await controller.GetQuestions(requestDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PaginatedResponse<QuestionExtendedDto>>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal("Page number and page size must be greater than 0.", badRequestResult.Value);
        mockQuestionService.Verify(s => s.GetQuestions(It.IsAny<QuestionsRequestDto>(), It.IsAny<ClaimsPrincipal>()),
            Times.Never);
    }

    [Fact]
    public async Task GetQuestions_ReturnsNotFound_WhenServiceReturnsNoItems()
    {
        // Arrange
        var requestDto = new QuestionsRequestDto { PageNumber = 1, PageSize = 50 };
        var emptyResponse = new PaginatedResponse<QuestionExtendedDto>
        {
            Items = new List<QuestionExtendedDto>(),
            TotalCount = 0,
            PageSize = 50
        };
        mockQuestionService.Setup(s => s.GetQuestions(requestDto, mockUser)).ReturnsAsync(emptyResponse);

        // Act
        var result = await controller.GetQuestions(requestDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PaginatedResponse<QuestionExtendedDto>>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal("No questions found.", notFoundResult.Value);
        mockQuestionService.Verify(s => s.GetQuestions(requestDto, mockUser), Times.Once);
    }

    [Fact]
    public async Task GetQuestions_ReturnsStatusCode500_WhenServiceThrowsException()
    {
        // Arrange
        var requestDto = new QuestionsRequestDto { PageNumber = 1, PageSize = 50 };
        var exception = new Exception("Database failure");
        mockQuestionService.Setup(s => s.GetQuestions(requestDto, mockUser)).ThrowsAsync(exception);
        string expectedLogMessage = "An error occurred while retrieving questions.";
        string expectedResponseMessage = expectedLogMessage;

        // Act
        var result = await controller.GetQuestions(requestDto);

        // Assert
        var actionResult = Assert.IsType<ActionResult<PaginatedResponse<QuestionExtendedDto>>>(result);
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal(expectedResponseMessage, objectResult.Value);
        mockQuestionService.Verify(s => s.GetQuestions(requestDto, mockUser), Times.Once);
        VerifyLogError(mockLogger, exception, expectedLogMessage);
    }

    // --- GetRandomUniqueQuestions Tests ---

    [Fact]
    public async Task GetRandomUniqueQuestions_ReturnsOkObjectResult_WithQuestions()
    {
        // Arrange
        var request = new UniqueQuestionsRequestDto();
        var questions = new List<QuestionExtendedDto> { new QuestionExtendedDto(1) { QuestionText = "Random Q1" } };
        mockQuestionService.Setup(s => s.GetRandomUniqueQuestions(request)).ReturnsAsync(questions);

        // Act
        var result = await controller.GetRandomUniqueQuestions(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<QuestionExtendedDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.Equal(questions, okResult.Value);
        mockQuestionService.Verify(s => s.GetRandomUniqueQuestions(request), Times.Once);
    }

    [Fact]
    public async Task GetRandomUniqueQuestions_ReturnsBadRequest_WhenRequestIsNull()
    {
        // Arrange
        UniqueQuestionsRequestDto? request = null;

        // Act
        var result = await controller.GetRandomUniqueQuestions(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<QuestionExtendedDto>>>(result);
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
        Assert.Equal("Get random unique questions data cannot be null.", badRequestResult.Value);
        mockQuestionService.Verify(s => s.GetRandomUniqueQuestions(It.IsAny<UniqueQuestionsRequestDto>()), Times.Never);
    }

    [Fact]
    public async Task GetRandomUniqueQuestions_ReturnsNotFound_WhenNoQuestionsFound()
    {
        // Arrange
        var request = new UniqueQuestionsRequestDto();
        mockQuestionService.Setup(s => s.GetRandomUniqueQuestions(request))
            .ReturnsAsync(new List<QuestionExtendedDto>());

        // Act
        var result = await controller.GetRandomUniqueQuestions(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<QuestionExtendedDto>>>(result);
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
        Assert.Equal("No questions found.", notFoundResult.Value);
        mockQuestionService.Verify(s => s.GetRandomUniqueQuestions(request), Times.Once);
    }

    [Fact]
    public async Task GetRandomUniqueQuestions_ReturnsStatusCode500_WhenServiceThrowsException()
    {
        // Arrange
        var request = new UniqueQuestionsRequestDto();
        var exception = new TimeoutException("Service timed out");
        mockQuestionService.Setup(s => s.GetRandomUniqueQuestions(request)).ThrowsAsync(exception);
        string expectedLogMessage = "An error occurred while retrieving random unique questions.";
        string expectedResponseMessage = expectedLogMessage;

        // Act
        var result = await controller.GetRandomUniqueQuestions(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<List<QuestionExtendedDto>>>(result);
        var objectResult = Assert.IsType<ObjectResult>(actionResult.Result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal(expectedResponseMessage, objectResult.Value);
        mockQuestionService.Verify(s => s.GetRandomUniqueQuestions(request), Times.Once);
        VerifyLogError(mockLogger, exception, expectedLogMessage);
    }

    // --- ApproveQuestion Tests ---

    [Fact]
    public async Task ApproveQuestion_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        int questionId = 1;
        mockQuestionService.Setup(s => s.ApproveQuestion(questionId, mockUser)).ReturnsAsync(true);

        // Act
        var result = await controller.ApproveQuestion(questionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Question approved successfully.", okResult.Value);
        mockQuestionService.Verify(s => s.ApproveQuestion(questionId, mockUser), Times.Once);
    }

    [Fact]
    public async Task ApproveQuestion_ReturnsNotFound_WhenQuestionNotFound()
    {
        // Arrange
        int questionId = 99;
        mockQuestionService.Setup(s => s.ApproveQuestion(questionId, mockUser)).ReturnsAsync(false);

        // Act
        var result = await controller.ApproveQuestion(questionId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Question with ID {questionId} not found.", notFoundResult.Value);
        mockQuestionService.Verify(s => s.ApproveQuestion(questionId, mockUser), Times.Once);
    }

    [Fact]
    public async Task ApproveQuestion_ReturnsStatusCode500_WhenServiceThrowsException()
    {
        // Arrange
        int questionId = 1;
        var exception = new InvalidOperationException("Approval conflict");
        mockQuestionService.Setup(s => s.ApproveQuestion(questionId, mockUser)).ThrowsAsync(exception);
        string expectedLogMessage = $"An error occurred while approving the question with ID {questionId}.";
        string expectedResponseMessage = expectedLogMessage;

        // Act
        var result = await controller.ApproveQuestion(questionId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal(expectedResponseMessage, objectResult.Value);
        mockQuestionService.Verify(s => s.ApproveQuestion(questionId, mockUser), Times.Once);
        VerifyLogError(mockLogger, exception, expectedLogMessage);
    }

    // --- CreateQuestion Tests ---

    [Fact]
    public async Task CreateQuestion_ReturnsCreated_WhenSuccessful()
    {
        // Arrange
        var newQuestionDto = new QuestionExtendedDto { QuestionText = "New Question?" };
        mockQuestionService.Setup(s => s.CreateQuestion(newQuestionDto, mockUser)).Returns(Task.CompletedTask);

        // Act
        var result = await controller.CreateQuestion(newQuestionDto);

        // Assert
        Assert.IsType<CreatedResult>(result);
        mockQuestionService.Verify(s => s.CreateQuestion(newQuestionDto, mockUser), Times.Once);
    }

    [Fact]
    public async Task CreateQuestion_ReturnsBadRequest_WhenInputIsNull()
    {
        // Arrange
        QuestionExtendedDto? newQuestionDto = null;

        // Act
        var result = await controller.CreateQuestion(newQuestionDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Question data cannot be null.", badRequestResult.Value);
        mockQuestionService.Verify(s => s.CreateQuestion(It.IsAny<QuestionExtendedDto>(), It.IsAny<ClaimsPrincipal>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateQuestion_ReturnsStatusCode500_WhenServiceThrowsException()
    {
        // Arrange
        var newQuestionDto = new QuestionExtendedDto { QuestionText = "New Question?" };
        var exception = new DbUpdateException("Save failed", new Exception());
        mockQuestionService.Setup(s => s.CreateQuestion(newQuestionDto, mockUser)).ThrowsAsync(exception);
        string expectedLogMessage = "An error occurred while saving the question.";
        string expectedResponseMessage = expectedLogMessage;

        // Act
        var result = await controller.CreateQuestion(newQuestionDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal(expectedResponseMessage, objectResult.Value);
        mockQuestionService.Verify(s => s.CreateQuestion(newQuestionDto, mockUser), Times.Once);
        VerifyLogError(mockLogger, exception, expectedLogMessage);
    }


    // --- UpdateQuestion Tests ---

    [Fact]
    public async Task UpdateQuestion_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        int questionId = 1;
        var updatedQuestionDto = new QuestionExtendedDto(questionId) { QuestionText = "Updated?" };
        mockQuestionService.Setup(s => s.UpdateQuestion(questionId, updatedQuestionDto, mockUser)).ReturnsAsync(true);

        // Act
        var result = await controller.UpdateQuestion(questionId, updatedQuestionDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal($"Question with ID {questionId} updated successfully.", okResult.Value);
        mockQuestionService.Verify(s => s.UpdateQuestion(questionId, updatedQuestionDto, mockUser), Times.Once);
    }

    [Fact]
    public async Task UpdateQuestion_ReturnsBadRequest_WhenInputIsNull()
    {
        // Arrange
        int questionId = 1;
        QuestionExtendedDto? updatedQuestionDto = null;

        // Act
        var result = await controller.UpdateQuestion(questionId, updatedQuestionDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Updated question data cannot be null.", badRequestResult.Value);
        mockQuestionService.Verify(
            s => s.UpdateQuestion(It.IsAny<int>(), It.IsAny<QuestionExtendedDto>(), It.IsAny<ClaimsPrincipal>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateQuestion_ReturnsNotFound_WhenQuestionNotFound()
    {
        // Arrange
        int questionId = 99;
        var updatedQuestionDto = new QuestionExtendedDto(questionId) { QuestionText = "Updated?" };
        mockQuestionService.Setup(s => s.UpdateQuestion(questionId, updatedQuestionDto, mockUser)).ReturnsAsync(false);

        // Act
        var result = await controller.UpdateQuestion(questionId, updatedQuestionDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Question with ID {questionId} not found.", notFoundResult.Value);
        mockQuestionService.Verify(s => s.UpdateQuestion(questionId, updatedQuestionDto, mockUser), Times.Once);
    }

    [Fact]
    public async Task UpdateQuestion_ReturnsStatusCode500_WhenServiceThrowsException()
    {
        // Arrange
        int questionId = 1;
        var updatedQuestionDto = new QuestionExtendedDto(questionId) { QuestionText = "Updated?" };
        var exception = new Exception("Update conflict");
        mockQuestionService.Setup(s => s.UpdateQuestion(questionId, updatedQuestionDto, mockUser))
            .ThrowsAsync(exception);
        string expectedLogMessage = $"An error occurred while updating the question with ID {questionId}.";
        string expectedResponseMessage = expectedLogMessage;

        // Act
        var result = await controller.UpdateQuestion(questionId, updatedQuestionDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal(expectedResponseMessage, objectResult.Value);
        mockQuestionService.Verify(s => s.UpdateQuestion(questionId, updatedQuestionDto, mockUser), Times.Once);
        VerifyLogError(mockLogger, exception, expectedLogMessage);
    }

    // --- DeleteQuestion Tests ---

    [Fact]
    public async Task DeleteQuestion_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        int questionId = 1;
        mockQuestionService.Setup(s => s.DeleteQuestion(questionId, mockUser)).ReturnsAsync(true);

        // Act
        var result = await controller.DeleteQuestion(questionId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal($"Question with ID {questionId} deleted successfully.", okResult.Value);
        mockQuestionService.Verify(s => s.DeleteQuestion(questionId, mockUser), Times.Once);
    }

    [Fact]
    public async Task DeleteQuestion_ReturnsNotFound_WhenQuestionNotFound()
    {
        // Arrange
        int questionId = 99;
        mockQuestionService.Setup(s => s.DeleteQuestion(questionId, mockUser)).ReturnsAsync(false);

        // Act
        var result = await controller.DeleteQuestion(questionId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Question with ID {questionId} not found.", notFoundResult.Value);
        mockQuestionService.Verify(s => s.DeleteQuestion(questionId, mockUser), Times.Once);
    }

    [Fact]
    public async Task DeleteQuestion_ReturnsStatusCode500_WhenServiceThrowsException()
    {
        // Arrange
        int questionId = 1;
        var exception = new InvalidOperationException("Cannot delete, related answers exist.");
        mockQuestionService.Setup(s => s.DeleteQuestion(questionId, mockUser)).ThrowsAsync(exception);
        string expectedLogMessage = $"An error occurred while deleting the question with ID {questionId}.";
        string expectedResponseMessage = expectedLogMessage;

        // Act
        var result = await controller.DeleteQuestion(questionId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal(expectedResponseMessage, objectResult.Value);
        mockQuestionService.Verify(s => s.DeleteQuestion(questionId, mockUser), Times.Once);
        VerifyLogError(mockLogger, exception, expectedLogMessage);
    }
}