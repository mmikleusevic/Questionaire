using Microsoft.AspNetCore.Identity;
using Moq;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Services;
using Shared.Models;
using Tests.Helper;

namespace Tests.ServiceTests;

public class UserServiceTests
{
    private readonly Mock<UserManager<User>> mockUserManager;
    private readonly List<User> usersData;
    private readonly IUserService userService;

    public UserServiceTests()
    {
        usersData = new List<User>();
        mockUserManager = UserManagerMockHelper.MockUserManager<User>();

        mockUserManager.Setup(m => m.Users).Returns(usersData.AsQueryable());

        userService = new UserService(mockUserManager.Object);
    }

    // --- GetUsers Tests ---

    [Fact]
    public async Task GetUsers_ReturnsMappedUserDtos_WhenUsersExist()
    {
        // Arrange
        var user1 = new User { UserName = "alice", Email = "alice@example.com" };
        var user2 = new User { UserName = "bob", Email = "bob@example.com" };
        usersData.AddRange(new[] { user1, user2 });

        var rolesUser1 = new List<string> { "User" };
        var rolesUser2 = new List<string> { "User", "Admin" };

        mockUserManager.Setup(m => m.GetRolesAsync(user1)).ReturnsAsync(rolesUser1);
        mockUserManager.Setup(m => m.GetRolesAsync(user2)).ReturnsAsync(rolesUser2);

        // Act
        List<UserDto> result = await userService.GetUsers();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var dto1 = result.FirstOrDefault(dto => dto.UserName == "alice");
        Assert.NotNull(dto1);
        Assert.Equal("alice@example.com", dto1.Email);
        Assert.Equal(rolesUser1, dto1.Roles);

        var dto2 = result.FirstOrDefault(dto => dto.UserName == "bob");
        Assert.NotNull(dto2);
        Assert.Equal("bob@example.com", dto2.Email);
        Assert.Equal(rolesUser2, dto2.Roles);

        mockUserManager.Verify(m => m.Users, Times.Once);
        mockUserManager.Verify(m => m.GetRolesAsync(user1), Times.Once);
        mockUserManager.Verify(m => m.GetRolesAsync(user2), Times.Once);
    }

    [Fact]
    public async Task GetUsers_ReturnsEmptyList_WhenNoUsersExist()
    {
        // Arrange

        // Act
        List<UserDto> result = await userService.GetUsers();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        mockUserManager.Verify(m => m.Users, Times.Once);
        mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task GetUsers_ThrowsWrappedException_WhenGetRolesAsyncFails()
    {
        // Arrange
        var user1 = new User { UserName = "failuser" };
        usersData.Add(user1);
        var exception = new Exception("Simulated identity error");

        mockUserManager.Setup(m => m.GetRolesAsync(user1)).ThrowsAsync(exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.GetUsers());

        Assert.Equal("An error occurred while retrieving users.", ex.Message);
        Assert.Equal(exception, ex.InnerException);
        mockUserManager.Verify(m => m.Users, Times.Once);
        mockUserManager.Verify(m => m.GetRolesAsync(user1), Times.Once);
    }

    [Fact]
    public async Task GetUsers_ThrowsWrappedException_WhenUsersPropertyAccessFails()
    {
        // Arrange
        var exception = new Exception("Simulated context error");

        mockUserManager.Setup(m => m.Users).Throws(exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.GetUsers());

        Assert.Equal("An error occurred while retrieving users.", ex.Message);
        Assert.Equal(exception, ex.InnerException);
        mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<User>()), Times.Never);
    }

    // --- UpdateUser Tests ---

    [Fact]
    public async Task UpdateUser_UpdatesRolesAndReturnsTrue_WhenUserFound()
    {
        // Arrange
        var userName = "userToUpdate";
        var user = new User { UserName = userName };
        var updatedUserDto = new UserDto { UserName = userName, Roles = new List<string> { "NewRole1", "NewRole2" } };
        var currentRoles = new List<string> { "OldRole" };

        mockUserManager.Setup(m => m.FindByNameAsync(userName)).ReturnsAsync(user);
        mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(currentRoles);
        mockUserManager.Setup(m => m.RemoveFromRolesAsync(user, currentRoles))
            .ReturnsAsync(IdentityResult.Success);
        mockUserManager.Setup(m => m.AddToRolesAsync(user, updatedUserDto.Roles))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        bool result = await userService.UpdateUser(updatedUserDto);

        // Assert
        Assert.True(result);
        mockUserManager.Verify(m => m.FindByNameAsync(userName), Times.Once);
        mockUserManager.Verify(m => m.GetRolesAsync(user), Times.Once);
        mockUserManager.Verify(m => m.RemoveFromRolesAsync(user, currentRoles), Times.Once);
        mockUserManager.Verify(m => m.AddToRolesAsync(user, updatedUserDto.Roles), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_ReturnsFalse_WhenUserNotFound()
    {
        // Arrange
        var userName = "notFoundUser";
        var updatedUserDto = new UserDto { UserName = userName, Roles = new List<string> { "SomeRole" } };

        mockUserManager.Setup(m => m.FindByNameAsync(userName)).ReturnsAsync((User)null);

        // Act
        bool result = await userService.UpdateUser(updatedUserDto);

        // Assert
        Assert.False(result);
        mockUserManager.Verify(m => m.FindByNameAsync(userName), Times.Once);
        mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<User>()), Times.Never);
        mockUserManager.Verify(m => m.RemoveFromRolesAsync(It.IsAny<User>(), It.IsAny<IList<string>>()), Times.Never);
        mockUserManager.Verify(m => m.AddToRolesAsync(It.IsAny<User>(), It.IsAny<IList<string>>()), Times.Never);
    }

    [Fact]
    public async Task UpdateUser_ThrowsWrappedException_WhenFindByNameAsyncFails()
    {
        // Arrange
        var userName = "failFindUser";
        var updatedUserDto = new UserDto { UserName = userName, Roles = new List<string> { "SomeRole" } };
        var exception = new Exception("Simulated find error");

        mockUserManager.Setup(m => m.FindByNameAsync(userName)).ThrowsAsync(exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.UpdateUser(updatedUserDto));
        Assert.Equal($"An error occurred while updating user with username {userName}.", ex.Message);
        Assert.Equal(exception, ex.InnerException);
    }

    [Fact]
    public async Task UpdateUser_ThrowsWrappedException_WhenRemoveFromRolesAsyncFails()
    {
        // Arrange
        var userName = "failRemoveRoleUser";
        var user = new User { UserName = userName };
        var updatedUserDto = new UserDto { UserName = userName, Roles = new List<string> { "NewRole" } };
        var currentRoles = new List<string> { "OldRole" };
        var exception = new Exception("Simulated role removal error");

        mockUserManager.Setup(m => m.FindByNameAsync(userName)).ReturnsAsync(user);
        mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(currentRoles);
        mockUserManager.Setup(m => m.RemoveFromRolesAsync(user, currentRoles))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.UpdateUser(updatedUserDto));
        Assert.Equal($"An error occurred while updating user with username {userName}.", ex.Message);
        Assert.Equal(exception, ex.InnerException);
        mockUserManager.Verify(m => m.AddToRolesAsync(It.IsAny<User>(), It.IsAny<IList<string>>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateUser_ThrowsWrappedException_WhenAddToRolesAsyncFails()
    {
        // Arrange
        var userName = "failAddRoleUser";
        var user = new User { UserName = userName };
        var updatedUserDto = new UserDto { UserName = userName, Roles = new List<string> { "NewRole" } };
        var currentRoles = new List<string> { "OldRole" };
        var exception = new Exception("Simulated role addition error");

        mockUserManager.Setup(m => m.FindByNameAsync(userName)).ReturnsAsync(user);
        mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(currentRoles);
        mockUserManager.Setup(m => m.RemoveFromRolesAsync(user, currentRoles))
            .ReturnsAsync(IdentityResult.Success);
        mockUserManager.Setup(m => m.AddToRolesAsync(user, updatedUserDto.Roles))
            .ThrowsAsync(exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.UpdateUser(updatedUserDto));
        Assert.Equal($"An error occurred while updating user with username {userName}.", ex.Message);
        Assert.Equal(exception, ex.InnerException);

        mockUserManager.Verify(m => m.FindByNameAsync(userName), Times.Once);
        mockUserManager.Verify(m => m.GetRolesAsync(user), Times.Once);
        mockUserManager.Verify(m => m.RemoveFromRolesAsync(user, currentRoles), Times.Once);
        mockUserManager.Verify(m => m.AddToRolesAsync(user, updatedUserDto.Roles), Times.Once);
    }

    // --- DeleteUser Tests ---

    [Fact]
    public async Task DeleteUser_DeletesUserAndReturnsTrue_WhenUserFoundAndDeletionSucceeds()
    {
        // Arrange
        var userName = "userToDelete";
        var user = new User { UserName = userName };

        mockUserManager.Setup(m => m.FindByNameAsync(userName)).ReturnsAsync(user);
        mockUserManager.Setup(m => m.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        bool result = await userService.DeleteUser(userName);

        // Assert
        Assert.True(result);
        mockUserManager.Verify(m => m.FindByNameAsync(userName), Times.Once);
        mockUserManager.Verify(m => m.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_ReturnsFalse_WhenUserFoundButDeletionFails()
    {
        // Arrange
        var userName = "userToDeleteFail";
        var user = new User { UserName = userName };
        var failureResult = IdentityResult.Failed(new IdentityError { Description = "Deletion failed" });

        mockUserManager.Setup(m => m.FindByNameAsync(userName)).ReturnsAsync(user);
        mockUserManager.Setup(m => m.DeleteAsync(user)).ReturnsAsync(failureResult);

        // Act
        bool result = await userService.DeleteUser(userName);

        // Assert
        Assert.False(result);
        mockUserManager.Verify(m => m.FindByNameAsync(userName), Times.Once);
        mockUserManager.Verify(m => m.DeleteAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_ReturnsFalse_WhenUserNotFound()
    {
        // Arrange
        var userName = "notFoundDeleteUser";

        mockUserManager.Setup(m => m.FindByNameAsync(userName)).ReturnsAsync((User)null);

        // Act
        bool result = await userService.DeleteUser(userName);

        // Assert
        Assert.False(result);
        mockUserManager.Verify(m => m.FindByNameAsync(userName), Times.Once);
        mockUserManager.Verify(m => m.DeleteAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUser_ThrowsWrappedException_WhenFindByNameAsyncFails()
    {
        // Arrange
        var userName = "failFindDeleteUser";
        var exception = new Exception("Simulated find error");

        mockUserManager.Setup(m => m.FindByNameAsync(userName)).ThrowsAsync(exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.DeleteUser(userName));
        Assert.Equal($"An error occurred while deleting user with username {userName}.", ex.Message);
        Assert.Equal(exception, ex.InnerException);
    }

    [Fact]
    public async Task DeleteUser_ThrowsWrappedException_WhenDeleteAsyncFails()
    {
        // Arrange
        var userName = "failDeleteUser";
        var user = new User { UserName = userName };
        var exception = new Exception("Simulated delete error");

        mockUserManager.Setup(m => m.FindByNameAsync(userName)).ReturnsAsync(user);
        mockUserManager.Setup(m => m.DeleteAsync(user)).ThrowsAsync(exception);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.DeleteUser(userName));
        Assert.Equal($"An error occurred while deleting user with username {userName}.", ex.Message);
        Assert.Equal(exception, ex.InnerException);
    }
}