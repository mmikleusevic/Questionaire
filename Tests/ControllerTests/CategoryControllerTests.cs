using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using QuestionaireApi.Controllers;
using QuestionaireApi.Interfaces;
using Shared.Models;

namespace Tests.ControllerTests;

public class CategoryControllerTests
{
    private readonly CategoryController controller;
    private readonly Mock<ICategoryService> mockCategoryService;
    private readonly Mock<ILogger<CategoryController>> mockLogger;

    public CategoryControllerTests()
    {
        mockCategoryService = new Mock<ICategoryService>();
        mockLogger = new Mock<ILogger<CategoryController>>();

        controller = new CategoryController(
            mockCategoryService.Object,
            mockLogger.Object);
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

    // --- GetCategories Tests ---

    [Fact]
    public async Task GetCategories_ReturnsOkObjectResult_WhenCategoriesExist()
    {
        // Arrange
        var expectedCategories = new CategoriesDto
        {
            FlatCategories = new List<CategoryExtendedDto> { new CategoryExtendedDto(1) { CategoryName = "Flat" } },
            NestedCategories = new List<CategoryExtendedDto> { new CategoryExtendedDto(2) { CategoryName = "Nested" } }
        };
        mockCategoryService.Setup(s => s.GetCategories()).ReturnsAsync(expectedCategories);

        // Act
        var result = await controller.GetCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedCategories, okResult.Value);
        mockCategoryService.Verify(s => s.GetCategories(), Times.Once);
    }

    [Fact]
    public async Task GetCategories_ReturnsNotFound_WhenNoCategoriesExist()
    {
        // Arrange
        var emptyCategories = new CategoriesDto { FlatCategories = new(), NestedCategories = new() };
        mockCategoryService.Setup(s => s.GetCategories()).ReturnsAsync(emptyCategories);

        // Act
        var result = await controller.GetCategories();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("No categories found.", notFoundResult.Value);
        mockCategoryService.Verify(s => s.GetCategories(), Times.Once);
    }

    [Fact]
    public async Task GetCategories_ReturnsStatusCode500_WhenServiceThrowsException()
    {
        // Arrange
        var exception = new InvalidOperationException("Database connection failed");
        mockCategoryService.Setup(s => s.GetCategories()).ThrowsAsync(exception);
        string expectedLogMessage = "An error occurred while retrieving categories.";
        string expectedResponseMessage = expectedLogMessage;

        // Act
        var result = await controller.GetCategories();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal(expectedResponseMessage, objectResult.Value);
        mockCategoryService.Verify(s => s.GetCategories(), Times.Once);
        VerifyLogError(mockLogger, exception, expectedLogMessage);
    }

    // --- GetNestedCategories Tests ---

    [Fact]
    public async Task GetNestedCategories_ReturnsOkObjectResult_WithCategories()
    {
        // Arrange
        var categories = new List<CategoryExtendedDto> { new CategoryExtendedDto(1) { CategoryName = "Nested 1" } };
        mockCategoryService.Setup(s => s.GetNestedCategories()).ReturnsAsync(categories);

        // Act
        var result = await controller.GetNestedCategories();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(categories, okResult.Value);
        mockCategoryService.Verify(s => s.GetNestedCategories(), Times.Once);
    }

    [Fact]
    public async Task GetNestedCategories_ReturnsNotFound_WhenNoCategoriesExist()
    {
        // Arrange
        mockCategoryService.Setup(s => s.GetNestedCategories()).ReturnsAsync(new List<CategoryExtendedDto>());

        // Act
        var result = await controller.GetNestedCategories();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("No categories found.", notFoundResult.Value);
        mockCategoryService.Verify(s => s.GetNestedCategories(), Times.Once);
    }

    [Fact]
    public async Task GetNestedCategories_ReturnsStatusCode500_WhenServiceThrowsException()
    {
        // Arrange
        var exception = new Exception("Service failure");
        mockCategoryService.Setup(s => s.GetNestedCategories()).ThrowsAsync(exception);
        string expectedLogMessage = "An error occurred while retrieving categories.";
        string expectedResponseMessage = expectedLogMessage;


        // Act
        var result = await controller.GetNestedCategories();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal(expectedResponseMessage, objectResult.Value);
        mockCategoryService.Verify(s => s.GetNestedCategories(), Times.Once);
        VerifyLogError(mockLogger, exception, expectedLogMessage);
    }


    // --- GetFlatCategories Tests ---

    [Fact]
    public async Task GetFlatCategories_ReturnsOkObjectResult_WithCategories()
    {
        // Arrange
        string searchQuery = "test";
        var categories = new List<CategoryExtendedDto> { new CategoryExtendedDto(1) { CategoryName = "Flat Test" } };
        mockCategoryService.Setup(s => s.GetFlatCategories(searchQuery)).ReturnsAsync(categories);

        // Act
        var result = await controller.GetFlatCategories(searchQuery);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(categories, okResult.Value);
        mockCategoryService.Verify(s => s.GetFlatCategories(searchQuery), Times.Once);
    }

    [Fact]
    public async Task GetFlatCategories_ReturnsNotFound_WhenNoCategoriesMatch()
    {
        // Arrange
        string searchQuery = "nomatch";
        mockCategoryService.Setup(s => s.GetFlatCategories(searchQuery)).ReturnsAsync(new List<CategoryExtendedDto>());

        // Act
        var result = await controller.GetFlatCategories(searchQuery);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("No categories found.", notFoundResult.Value);
        mockCategoryService.Verify(s => s.GetFlatCategories(searchQuery), Times.Once);
    }

    [Fact]
    public async Task GetFlatCategories_ReturnsStatusCode500_WhenServiceThrowsException()
    {
        // Arrange
        string searchQuery = "test";
        var exception = new TimeoutException("Query timed out");
        mockCategoryService.Setup(s => s.GetFlatCategories(searchQuery)).ThrowsAsync(exception);
        string expectedLogMessage = "An error occurred while retrieving categories.";
        string expectedResponseMessage = expectedLogMessage;

        // Act
        var result = await controller.GetFlatCategories(searchQuery);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal(expectedResponseMessage, objectResult.Value);
        mockCategoryService.Verify(s => s.GetFlatCategories(searchQuery), Times.Once);
        VerifyLogError(mockLogger, exception, expectedLogMessage);
    }

    // --- CreateCategory Tests ---

    [Fact]
    public async Task CreateCategory_ReturnsCreated_WhenSuccessful()
    {
        // Arrange
        var newCategoryDto = new CategoryExtendedDto { CategoryName = "New Category" };
        mockCategoryService.Setup(s => s.CreateCategory(newCategoryDto)).Returns(Task.CompletedTask);

        // Act
        var result = await controller.CreateCategory(newCategoryDto);

        // Assert
        Assert.IsType<CreatedResult>(result);
        mockCategoryService.Verify(s => s.CreateCategory(newCategoryDto), Times.Once);
    }

    [Fact]
    public async Task CreateCategory_ReturnsBadRequest_WhenInputIsNull()
    {
        // Arrange
        CategoryExtendedDto? newCategoryDto = null;

        // Act
        var result = await controller.CreateCategory(newCategoryDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Category data cannot be null.", badRequestResult.Value);
        mockCategoryService.Verify(s => s.CreateCategory(It.IsAny<CategoryExtendedDto>()), Times.Never);
    }

    [Fact]
    public async Task CreateCategory_ReturnsStatusCode500_WhenServiceThrowsException()
    {
        // Arrange
        var newCategoryDto = new CategoryExtendedDto { CategoryName = "New Category" };
        var exception = new DbUpdateException("Failed to save", new Exception());
        mockCategoryService.Setup(s => s.CreateCategory(newCategoryDto)).ThrowsAsync(exception);
        string expectedLogMessage = "An error occurred while creating the category.";
        string expectedResponseMessage = expectedLogMessage;

        // Act
        var result = await controller.CreateCategory(newCategoryDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal(expectedResponseMessage, objectResult.Value);
        mockCategoryService.Verify(s => s.CreateCategory(newCategoryDto), Times.Once);
        VerifyLogError(mockLogger, exception, expectedLogMessage);
    }


    // --- UpdateCategory Tests ---

    [Fact]
    public async Task UpdateCategory_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        int categoryId = 1;
        var updatedCategoryDto = new CategoryExtendedDto(categoryId) { CategoryName = "Updated Name" };
        mockCategoryService.Setup(s => s.UpdateCategory(categoryId, updatedCategoryDto)).ReturnsAsync(true);

        // Act
        var result = await controller.UpdateCategory(categoryId, updatedCategoryDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal($"Category with ID {categoryId} updated successfully.", okResult.Value);
        mockCategoryService.Verify(s => s.UpdateCategory(categoryId, updatedCategoryDto), Times.Once);
    }

    [Fact]
    public async Task UpdateCategory_ReturnsNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        int categoryId = 99;
        var updatedCategoryDto = new CategoryExtendedDto(categoryId) { CategoryName = "Doesn't Exist" };
        mockCategoryService.Setup(s => s.UpdateCategory(categoryId, updatedCategoryDto)).ReturnsAsync(false);

        // Act
        var result = await controller.UpdateCategory(categoryId, updatedCategoryDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Category with ID {categoryId} not found.", notFoundResult.Value);
        mockCategoryService.Verify(s => s.UpdateCategory(categoryId, updatedCategoryDto), Times.Once);
    }

    [Fact]
    public async Task UpdateCategory_ReturnsBadRequest_WhenInputIsNull()
    {
        // Arrange
        int categoryId = 1;
        CategoryExtendedDto? updatedCategoryDto = null;

        // Act
        var result = await controller.UpdateCategory(categoryId, updatedCategoryDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Updated category data cannot be null.", badRequestResult.Value);
        mockCategoryService.Verify(s => s.UpdateCategory(It.IsAny<int>(), It.IsAny<CategoryExtendedDto>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateCategory_ReturnsStatusCode500_WhenServiceThrowsException()
    {
        // Arrange
        int categoryId = 1;
        var updatedCategoryDto = new CategoryExtendedDto(categoryId) { CategoryName = "Update Fail" };
        var exception = new Exception("Concurrency issue");
        mockCategoryService.Setup(s => s.UpdateCategory(categoryId, updatedCategoryDto)).ThrowsAsync(exception);
        string expectedLogMessage = $"An error occurred while updating the category with ID {categoryId}.";
        string expectedResponseMessage = expectedLogMessage;

        // Act
        var result = await controller.UpdateCategory(categoryId, updatedCategoryDto);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal(expectedResponseMessage, objectResult.Value);
        mockCategoryService.Verify(s => s.UpdateCategory(categoryId, updatedCategoryDto), Times.Once);
        VerifyLogError(mockLogger, exception, expectedLogMessage);
    }

    // --- DeleteCategory Tests ---

    [Fact]
    public async Task DeleteCategory_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        int categoryId = 5;
        mockCategoryService.Setup(s => s.DeleteCategory(categoryId)).ReturnsAsync(true);

        // Act
        var result = await controller.DeleteCategory(categoryId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal($"Category with ID {categoryId} deleted successfully.", okResult.Value);
        mockCategoryService.Verify(s => s.DeleteCategory(categoryId), Times.Once);
    }

    [Fact]
    public async Task DeleteCategory_ReturnsNotFound_WhenCategoryDoesNotExist()
    {
        // Arrange
        int categoryId = 99;
        mockCategoryService.Setup(s => s.DeleteCategory(categoryId)).ReturnsAsync(false);

        // Act
        var result = await controller.DeleteCategory(categoryId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"Category with ID {categoryId} not found.", notFoundResult.Value);
        mockCategoryService.Verify(s => s.DeleteCategory(categoryId), Times.Once);
    }

    [Fact]
    public async Task DeleteCategory_ReturnsStatusCode500_WhenServiceThrowsException()
    {
        // Arrange
        int categoryId = 5;
        var exception = new Exception("Foreign key constraint");
        mockCategoryService.Setup(s => s.DeleteCategory(categoryId)).ThrowsAsync(exception);
        string expectedLogMessage = $"An error occurred while deleting the category with ID {categoryId}.";
        string expectedResponseMessage = expectedLogMessage;

        // Act
        var result = await controller.DeleteCategory(categoryId);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        Assert.Equal(expectedResponseMessage, objectResult.Value);
        mockCategoryService.Verify(s => s.DeleteCategory(categoryId), Times.Once);
        VerifyLogError(mockLogger, exception, expectedLogMessage);
    }
}