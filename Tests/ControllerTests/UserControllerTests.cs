using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QuestionaireApi.Controllers;
using QuestionaireApi.Interfaces;
using Shared.Models;

namespace Tests.ControllerTests;

public class UserControllerTests
{
    private readonly UserController controller;
    private readonly Mock<ILogger<UserController>> mockLogger;
    private readonly Mock<IUserService> mockUserService;

    public UserControllerTests()
    {
        mockUserService = new Mock<IUserService>();
        mockLogger = new Mock<ILogger<UserController>>();

        controller = new UserController(
            mockUserService.Object,
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

    // --- GetUsers Tests ---

    [Fact]
    public async Task GetUsers_ReturnsOkObjectResult_WhenUsersExist()
    {
        // Arrange
        var expectedUsers = new List<UserDto>
        {
            new UserDto
            {
                UserName = "user1", Email = "user1@test.com",
                Roles = new List<RoleDto> { new RoleDto { RoleName = "User" } }
            },
            new UserDto
            {
                UserName = "admin1", Email = "admin1@test.com",
                Roles = new List<RoleDto> { new RoleDto { RoleName = "Admin" } }
            }
        };
        mockUserService.Setup(s => s.GetUsers()).ReturnsAsync(expectedUsers);

        // Act
        var result = await controller.GetUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualUsers = Assert.IsAssignableFrom<List<UserDto>>(okResult.Value);
        Assert.Equal(expectedUsers, actualUsers);
        mockUserService.Verify(s => s.GetUsers(), Times.Once);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task GetUsers_ReturnsNotFound_WhenNoUsersExist()
    {
        // Arrange
        mockUserService.Setup(s => s.GetUsers()).ReturnsAsync(new List<UserDto>());

        // Act
        var result = await controller.GetUsers();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("No users found.", notFoundResult.Value);
        mockUserService.Verify(s => s.GetUsers(), Times.Once);
    }

    [Fact]
    public async Task GetUsers_ReturnsStatusCode500_WhenServiceThrowsException()
    {
        // Arrange
        var exception = new InvalidOperationException("Identity database unavailable.");
        mockUserService.Setup(s => s.GetUsers()).ThrowsAsync(exception);
        string expectedLogMessage = "An error occurred while retrieving users.";
        string expectedResponseMessage = expectedLogMessage;

        // Act
        var result = await controller.GetUsers();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal(expectedResponseMessage, objectResult.Value);
        mockUserService.Verify(s => s.GetUsers(), Times.Once);
        VerifyLogError(mockLogger, exception, expectedLogMessage);
    }

    // --- UpdateUserRole Tests ---

    [Fact]
    public async Task UpdateUserRole_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        var userToUpdate = new UserDto
            { UserName = "testuser", Roles = new List<RoleDto> { new RoleDto { RoleName = "Admin" } } };
        mockUserService.Setup(s => s.UpdateUser(userToUpdate)).ReturnsAsync(true);

        // Act
        var result = await controller.UpdateUserRole(userToUpdate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal($"User with username {userToUpdate.UserName} updated successfully.", okResult.Value);
        mockUserService.Verify(s => s.UpdateUser(userToUpdate), Times.Once);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateUserRole_ReturnsBadRequest_WhenInputIsNull()
    {
        // Arrange
        UserDto? userToUpdate = null;

        // Act
        var result = await controller.UpdateUserRole(userToUpdate);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Updated user data cannot be null.", badRequestResult.Value);
        mockUserService.Verify(s => s.UpdateUser(It.IsAny<UserDto>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUserRole_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        var userToUpdate = new UserDto
            { UserName = "nonexistent", Roles = new List<RoleDto> { new RoleDto { RoleName = "User" } } };
        mockUserService.Setup(s => s.UpdateUser(userToUpdate)).ReturnsAsync(false);

        // Act
        var result = await controller.UpdateUserRole(userToUpdate);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"User with username {userToUpdate.UserName} not found.", notFoundResult.Value);
        mockUserService.Verify(s => s.UpdateUser(userToUpdate), Times.Once);
    }

    [Fact]
    public async Task UpdateUserRole_ReturnsStatusCode500_WhenServiceThrowsException()
    {
        // Arrange
        var userToUpdate = new UserDto
            { UserName = "testuser", Roles = new List<RoleDto> { new RoleDto { RoleName = "Admin" } } };
        var exception = new Exception("Concurrency conflict during update.");
        mockUserService.Setup(s => s.UpdateUser(userToUpdate)).ThrowsAsync(exception);
        string expectedLogMessage =
            $"An error occurred while updating the user with username {userToUpdate?.UserName}.";
        string expectedResponseMessage = expectedLogMessage;

        // Act
        var result = await controller.UpdateUserRole(userToUpdate);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal(expectedResponseMessage, objectResult.Value);
        mockUserService.Verify(s => s.UpdateUser(userToUpdate), Times.Once);
        VerifyLogError(mockLogger, exception, expectedLogMessage);
    }

    [Fact]
    public async Task UpdateUserRole_ReturnsStatusCode500_WhenServiceThrowsExceptionAndInputIsNull()
    {
        // Arrange
        UserDto? userToUpdate = null;

        // Act
        var result = await controller.UpdateUserRole(userToUpdate);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Updated user data cannot be null.", badRequestResult.Value);
        mockUserService.Verify(s => s.UpdateUser(It.IsAny<UserDto>()),
            Times.Never);
    }

    // --- DeleteUser Tests ---

    [Fact]
    public async Task DeleteUser_ReturnsOk_WhenSuccessful()
    {
        // Arrange
        string userNameToDelete = "userToDelete";
        mockUserService.Setup(s => s.DeleteUser(userNameToDelete)).ReturnsAsync(true);

        // Act
        var result = await controller.DeleteUser(userNameToDelete);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal($"User with username {userNameToDelete} deleted successfully.", okResult.Value);
        mockUserService.Verify(s => s.DeleteUser(userNameToDelete), Times.Once);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteUser_ReturnsNotFound_WhenUserNotFound()
    {
        // Arrange
        string userNameToDelete = "nonexistentUser";
        mockUserService.Setup(s => s.DeleteUser(userNameToDelete)).ReturnsAsync(false);

        // Act
        var result = await controller.DeleteUser(userNameToDelete);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal($"User with username {userNameToDelete} not found or user was not deleted.", notFoundResult.Value);
        mockUserService.Verify(s => s.DeleteUser(userNameToDelete), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_ReturnsStatusCode500_WhenServiceThrowsException()
    {
        // Arrange
        string userNameToDelete = "userToDelete";
        var exception = new Exception("Error accessing identity store during delete.");
        mockUserService.Setup(s => s.DeleteUser(userNameToDelete)).ThrowsAsync(exception);
        string expectedLogMessage = $"An error occurred while deleting the user with username {userNameToDelete}.";
        string expectedResponseMessage = expectedLogMessage;

        // Act
        var result = await controller.DeleteUser(userNameToDelete);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal(expectedResponseMessage, objectResult.Value);
        mockUserService.Verify(s => s.DeleteUser(userNameToDelete), Times.Once);
        VerifyLogError(mockLogger, exception, expectedLogMessage);
    }
}