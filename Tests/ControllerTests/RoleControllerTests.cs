using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using QuestionaireApi.Controllers;
using QuestionaireApi.Interfaces;
using Shared.Models;

namespace Tests.ControllerTests;

public class RoleControllerTests
{
    private readonly RoleController controller;
    private readonly Mock<ILogger<UserController>> mockLogger;
    private readonly Mock<IRoleService> mockRoleService;

    public RoleControllerTests()
    {
        mockRoleService = new Mock<IRoleService>();
        mockLogger = new Mock<ILogger<UserController>>();

        controller = new RoleController(
            mockRoleService.Object,
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

    // --- GetRoles Tests ---

    [Fact]
    public async Task GetRoles_ReturnsOkObjectResult_WhenRolesExist()
    {
        // Arrange
        var expectedRoles = new List<RoleDto>
        {
            new RoleDto { RoleName = "Admin" },
            new RoleDto { RoleName = "User" },
            new RoleDto { RoleName = "SuperAdmin" }
        };
        mockRoleService.Setup(s => s.GetRoles()).ReturnsAsync(expectedRoles);

        // Act
        var result = await controller.GetRoles();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualRoles = Assert.IsAssignableFrom<IList<RoleDto>>(okResult.Value);
        Assert.Equal(expectedRoles, actualRoles);
        mockRoleService.Verify(s => s.GetRoles(), Times.Once);
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
    public async Task GetRoles_ReturnsNotFound_WhenNoRolesExist()
    {
        // Arrange
        var emptyRoles = new List<RoleDto>();
        mockRoleService.Setup(s => s.GetRoles()).ReturnsAsync(emptyRoles);

        // Act
        var result = await controller.GetRoles();

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("No roles found.", notFoundResult.Value);
        mockRoleService.Verify(s => s.GetRoles(), Times.Once);
    }

    [Fact]
    public async Task GetRoles_ReturnsStatusCode500_WhenServiceThrowsException()
    {
        // Arrange
        var exception = new InvalidOperationException("Could not connect to identity store.");
        mockRoleService.Setup(s => s.GetRoles()).ThrowsAsync(exception);

        string expectedLogMessage = "An error occurred while retrieving roles.";
        string expectedResponseMessage = expectedLogMessage;

        // Act
        var result = await controller.GetRoles();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
        Assert.Equal(expectedResponseMessage, objectResult.Value);
        mockRoleService.Verify(s => s.GetRoles(), Times.Once);

        VerifyLogError(mockLogger, exception, expectedLogMessage);
    }
}