using Microsoft.AspNetCore.Identity;
using Moq;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Services;
using Shared.Models;
using Tests.Helper;

namespace Tests.ServiceTests;

public class RoleServiceTests
{
    private readonly Mock<RoleManager<IdentityRole>> mockRoleManager;
    private readonly List<IdentityRole> rolesData;
    private readonly IRoleService roleService;

    public RoleServiceTests()
    {
        rolesData = new List<IdentityRole>();

        mockRoleManager = RoleManagerMockHelper.MockRoleManager(rolesData);

        roleService = new RoleService(mockRoleManager.Object);
    }

    // --- GetRoles Tests ---

    [Fact]
    public async Task GetRoles_ReturnsListOfRoleNames_WhenRolesExist()
    {
        // Arrange
        rolesData.AddRange(new[]
        {
            new IdentityRole { Name = "Admin" },
            new IdentityRole { Name = "User" },
            new IdentityRole { Name = "SuperAdmin" }
        });

        var expectedRoleNames = new List<string> { "Admin", "User", "SuperAdmin" };

        // Act
        IList<RoleDto> result = await roleService.GetRoles();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRoleNames.Count, result.Count);

        Assert.Contains("Admin", result.Select(r => r.RoleName));
        Assert.Contains("User", result.Select(r => r.RoleName));
        Assert.Contains("SuperAdmin", result.Select(r => r.RoleName));

        mockRoleManager.Verify(m => m.Roles, Times.Once);
    }

    [Fact]
    public async Task GetRoles_ReturnsEmptyList_WhenNoRolesExist()
    {
        // Arrange

        // Act
        IList<RoleDto> result = await roleService.GetRoles();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        mockRoleManager.Verify(m => m.Roles, Times.Once);
    }

    [Fact]
    public async Task GetRoles_ThrowsWrappedException_WhenRoleManagerQueryFails()
    {
        // Arrange
        var dbException = new Exception("Simulated database connection error");

        mockRoleManager.Setup(m => m.Roles).Throws(dbException);

        var serviceWithThrowingManager = new RoleService(mockRoleManager.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => serviceWithThrowingManager.GetRoles());

        Assert.Equal("An error occurred while retrieving roles.", ex.Message);
        Assert.Equal(dbException, ex.InnerException);
        mockRoleManager.Verify(m => m.Roles, Times.AtLeastOnce);
    }

    [Fact]
    public async Task GetRoles_FiltersOutNullRoleNames()
    {
        // Arrange
        rolesData.AddRange(new[]
        {
            new IdentityRole { Name = "Admin" },
            new IdentityRole { Name = null },
            new IdentityRole { Name = "User" }
        });

        var expectedRoleNames = new List<string> { "Admin", "User" };

        // Act
        IList<RoleDto> result = await roleService.GetRoles();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedRoleNames.Count, result.Count);
        Assert.Contains("Admin", result.Select(r => r.RoleName));
        Assert.Contains("User", result.Select(r => r.RoleName));
        Assert.DoesNotContain(null, result);

        mockRoleManager.Verify(m => m.Roles, Times.Once);
    }
}