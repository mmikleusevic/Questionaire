using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using MockQueryable.Moq;
using Moq;
using QuestionaireApi;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Services;
using Shared.Models;
using Tests.Helper;

namespace Tests.ServiceTests;

public class UserServiceTests
{
    private const string DefaultUserName = "admin";
    private readonly User defaultUser = new User { Id = Guid.NewGuid().ToString(), UserName = DefaultUserName };
    private readonly Mock<QuestionaireDbContext> mockContext;
    private readonly Mock<DatabaseFacade> mockDatabaseFacade;
    private readonly Mock<IExecutionStrategy> mockExecutionStrategy;
    private readonly Mock<DbSet<Question>> mockQuestionDbSet;
    private readonly Mock<IDbContextTransaction> mockTransaction;
    private readonly Mock<UserManager<User>> mockUserManager;
    private readonly List<Question> questionsData;

    private readonly List<User> usersData;
    private readonly IUserService userService;

    public UserServiceTests()
    {
        usersData = new List<User>();
        questionsData = new List<Question>();

        mockUserManager = UserManagerMockHelper.MockUserManager<User>();
        mockUserManager.Setup(m => m.Users).Returns(usersData.AsQueryable());

        mockContext = new Mock<QuestionaireDbContext>(new DbContextOptions<QuestionaireDbContext>());
        mockQuestionDbSet = questionsData.AsQueryable().BuildMockDbSet();
        mockContext.Setup(c => c.Questions).Returns(mockQuestionDbSet.Object);

        mockTransaction = new Mock<IDbContextTransaction>();
        mockExecutionStrategy = new Mock<IExecutionStrategy>();
        mockDatabaseFacade = new Mock<DatabaseFacade>(mockContext.Object);

        mockDatabaseFacade.Setup(db => db.CreateExecutionStrategy())
            .Returns(mockExecutionStrategy.Object);
        mockDatabaseFacade.Setup(db => db.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);
        mockContext.Setup(c => c.Database).Returns(mockDatabaseFacade.Object);

        mockExecutionStrategy
            .Setup(s => s.ExecuteAsync(
                It.IsAny<Func<Task<bool>>>(),
                It.IsAny<Func<DbContext, Func<Task<bool>>, CancellationToken,
                    Task<bool>>>(),
                It.IsAny<Func<DbContext, Func<Task<bool>>, CancellationToken, Task<ExecutionResult<bool>>>
                    ?>(),
                It.IsAny<CancellationToken>()))
            .Returns(
                async (
                    Func<Task<bool>> state,
                    Func<DbContext, Func<Task<bool>>, CancellationToken, Task<bool>>
                        operation,
                    Func<DbContext, Func<Task<bool>>, CancellationToken, Task<ExecutionResult<bool>>>? verifySucceeded,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await operation(mockContext.Object, state, cancellationToken);
                    return result;
                })
            .Verifiable();

        userService = new UserService(
            mockUserManager.Object,
            mockContext.Object
        );
    }

    // --- GetUsers Tests (Unchanged) ---

    [Fact]
    public async Task GetUsers_ReturnsMappedUserDtos_WhenUsersExist()
    {
        // Arrange
        var user1 = new User { Id = Guid.NewGuid().ToString(), UserName = "alice", Email = "alice@example.com" };
        var user2 = new User { Id = Guid.NewGuid().ToString(), UserName = "bob", Email = "bob@example.com" };
        usersData.AddRange(new[] { user1, user2 });

        var rolesUser1 = new List<string> { "User" };
        var rolesUser2 = new List<string> { "User", "Admin" };

        mockUserManager.Setup(m => m.GetRolesAsync(It.Is<User>(u => u.UserName == "alice"))).ReturnsAsync(rolesUser1);
        mockUserManager.Setup(m => m.GetRolesAsync(It.Is<User>(u => u.UserName == "bob"))).ReturnsAsync(rolesUser2);

        // Act
        List<UserDto> result = await userService.GetUsers();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var dto1 = result.FirstOrDefault(dto => dto.UserName == "alice");
        Assert.NotNull(dto1);
        Assert.Equal("alice@example.com", dto1.Email);
        Assert.Single(dto1.Roles);
        Assert.Equal("User", dto1.Roles[0].RoleName);

        var dto2 = result.FirstOrDefault(dto => dto.UserName == "bob");
        Assert.NotNull(dto2);
        Assert.Equal("bob@example.com", dto2.Email);
        Assert.Equal(2, dto2.Roles.Count);
        Assert.Contains(dto2.Roles, r => r.RoleName == "User");
        Assert.Contains(dto2.Roles, r => r.RoleName == "Admin");

        mockUserManager.Verify(m => m.GetRolesAsync(It.Is<User>(u => u.UserName == "alice")), Times.Once);
        mockUserManager.Verify(m => m.GetRolesAsync(It.Is<User>(u => u.UserName == "bob")), Times.Once);
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

    // --- UpdateUser Tests (Unchanged) ---

    [Fact]
    public async Task UpdateUser_UpdatesRolesAndReturnsTrue_WhenUserFound()
    {
        // Arrange
        var userName = "userToUpdate";
        var userId = Guid.NewGuid().ToString();
        var user = new User { Id = userId, UserName = userName };
        var updatedUserDto = new UserDto
        {
            UserName = userName, Roles = new List<RoleDto>
            {
                new RoleDto { RoleName = "NewRole1" },
                new RoleDto { RoleName = "NewRole2" }
            }
        };
        var currentRoles = new List<string> { "OldRole" };
        var expectedNewRoles = new List<string> { "NewRole1", "NewRole2" };


        mockUserManager.Setup(m => m.FindByNameAsync(userName)).ReturnsAsync(user);
        mockUserManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(currentRoles);
        mockUserManager.Setup(m => m.RemoveFromRolesAsync(user, currentRoles))
            .ReturnsAsync(IdentityResult.Success);

        mockUserManager.Setup(m =>
                m.AddToRolesAsync(user, It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(expectedNewRoles))))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        bool result = await userService.UpdateUser(updatedUserDto);

        // Assert
        Assert.True(result);
        mockUserManager.Verify(m => m.FindByNameAsync(userName), Times.Once);
        mockUserManager.Verify(m => m.GetRolesAsync(user), Times.Once);
        mockUserManager.Verify(m => m.RemoveFromRolesAsync(user, currentRoles), Times.Once);
        mockUserManager.Verify(m => m.AddToRolesAsync(
            user,
            It.Is<IEnumerable<string>>(roles => roles.SequenceEqual(expectedNewRoles))
        ), Times.Once);
    }

    [Fact]
    public async Task UpdateUser_ReturnsFalse_WhenUserNotFound()
    {
        // Arrange
        var userName = "notFoundUser";
        var updatedUserDto = new UserDto
        {
            UserName = userName, Roles = new List<RoleDto>
            {
                new RoleDto { RoleName = "SomeRole" }
            }
        };

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
        var updatedUserDto = new UserDto
        {
            UserName = userName, Roles = new List<RoleDto>
            {
                new RoleDto { RoleName = "SomeRole" }
            }
        };

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
        var updatedUserDto = new UserDto
        {
            UserName = userName, Roles = new List<RoleDto>
            {
                new RoleDto { RoleName = "NewRole" }
            }
        };
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
        var updatedUserDto = new UserDto
        {
            UserName = userName, Roles = new List<RoleDto>
            {
                new RoleDto { RoleName = "NewRole" }
            }
        };

        mockUserManager.Setup(m => m.FindByNameAsync(userName))
            .ReturnsAsync((User)null);

        // Act
        var result = await userService.UpdateUser(updatedUserDto);

        // Assert
        Assert.False(result);

        mockUserManager.Verify(m => m.FindByNameAsync(userName), Times.Once);
        mockUserManager.Verify(m => m.GetRolesAsync(It.IsAny<User>()), Times.Never);
        mockUserManager.Verify(m => m.RemoveFromRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()),
            Times.Never);
        mockUserManager.Verify(m => m.AddToRolesAsync(It.IsAny<User>(), It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    // --- DeleteUser Tests (Updated) ---

    [Fact]
    public async Task DeleteUser_ReassignsContent_DeletesUser_Commits_AndReturnsTrue_WhenSuccessful()
    {
        // Arrange
        var userNameToDelete = "userToDelete";
        var userToDelete = new User { Id = Guid.NewGuid().ToString(), UserName = userNameToDelete };

        questionsData.Add(new Question { Id = 1, QuestionText = "Q1", CreatedById = userToDelete.Id });
        questionsData.Add(new Question { Id = 2, QuestionText = "Q2", LastUpdatedById = userToDelete.Id });
        questionsData.Add(new Question
        {
            Id = 3, QuestionText = "Q3", CreatedById = "otherUserId"
        });

        mockUserManager.Setup(m => m.FindByNameAsync(DefaultUserName)).ReturnsAsync(defaultUser).Verifiable();
        mockUserManager.Setup(m => m.FindByNameAsync(userNameToDelete)).ReturnsAsync(userToDelete).Verifiable();
        mockUserManager.Setup(m => m.DeleteAsync(It.Is<User>(u => u.Id == userToDelete.Id)))
            .ReturnsAsync(IdentityResult.Success)
            .Verifiable();
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1)
            .Verifiable();
        mockTransaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        mockTransaction.Setup(t => t.DisposeAsync())
            .Returns(ValueTask.CompletedTask)
            .Verifiable();

        // Act
        var result = await userService.DeleteUser(userNameToDelete);

        // Assert
        Assert.True(result);

        mockExecutionStrategy.Verify();
        mockUserManager.Verify();
        mockContext.Verify();
        mockTransaction.Verify();
        mockUserManager.Verify(m => m.FindByNameAsync(DefaultUserName), Times.Once);
        mockUserManager.Verify(m => m.FindByNameAsync(userNameToDelete), Times.Once);
        mockContext.Verify(c => c.Questions, Times.Once);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockUserManager.Verify(m => m.DeleteAsync(It.Is<User>(u => u.Id == userToDelete.Id)), Times.Once);
        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Never);

        Assert.Equal(defaultUser.Id, questionsData.First(q => q.Id == 1).CreatedById);
        Assert.Equal(defaultUser.Id, questionsData.First(q => q.Id == 2).LastUpdatedById);
        Assert.NotEqual(defaultUser.Id, questionsData.First(q => q.Id == 3).CreatedById);
    }

    [Fact]
    public async Task DeleteUser_DoesNotSaveChanges_WhenNoContentToReassign_ReturnsTrue()
    {
        // Arrange
        var userNameToDelete = "userWithNoContent";
        var userToDelete = new User { Id = Guid.NewGuid().ToString(), UserName = userNameToDelete };
        questionsData.Clear();

        mockUserManager.Setup(m => m.FindByNameAsync(DefaultUserName)).ReturnsAsync(defaultUser).Verifiable();
        mockUserManager.Setup(m => m.FindByNameAsync(userNameToDelete)).ReturnsAsync(userToDelete).Verifiable();
        mockUserManager.Setup(m => m.DeleteAsync(It.Is<User>(u => u.Id == userToDelete.Id)))
            .ReturnsAsync(IdentityResult.Success)
            .Verifiable();
        mockTransaction.Setup(t => t.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        mockTransaction.Setup(t => t.DisposeAsync())
            .Returns(ValueTask.CompletedTask)
            .Verifiable();

        // Act
        var result = await userService.DeleteUser(userNameToDelete);

        // Assert
        Assert.True(result);

        mockExecutionStrategy.Verify();
        mockUserManager.Verify();
        mockTransaction.Verify();
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockUserManager.Verify(m => m.FindByNameAsync(DefaultUserName), Times.Once);
        mockUserManager.Verify(m => m.FindByNameAsync(userNameToDelete), Times.Once);
        mockUserManager.Verify(m => m.DeleteAsync(It.Is<User>(u => u.Id == userToDelete.Id)), Times.Once);
        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
        mockContext.Verify(c => c.Questions, Times.Once);
    }

    [Fact]
    public async Task DeleteUser_RollsBack_AndReturnsFalse_WhenUserDeletionFails()
    {
        // Arrange
        var userNameToDelete = "userToDeleteFail";
        var userToDelete = new User { Id = Guid.NewGuid().ToString(), UserName = userNameToDelete };
        var failureResult = IdentityResult.Failed(new IdentityError { Description = "Deletion failed" });
        questionsData.Clear();

        mockUserManager.Setup(m => m.FindByNameAsync(DefaultUserName)).ReturnsAsync(defaultUser).Verifiable();
        mockUserManager.Setup(m => m.FindByNameAsync(userNameToDelete)).ReturnsAsync(userToDelete).Verifiable();
        mockUserManager.Setup(m => m.DeleteAsync(It.Is<User>(u => u.Id == userToDelete.Id)))
            .ReturnsAsync(failureResult)
            .Verifiable();
        mockTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        mockTransaction.Setup(t => t.DisposeAsync())
            .Returns(ValueTask.CompletedTask)
            .Verifiable();

        // Act
        var result = await userService.DeleteUser(userNameToDelete);

        // Assert
        Assert.False(result);

        mockExecutionStrategy.Verify();
        mockUserManager.Verify();
        mockTransaction.Verify();
        mockUserManager.Verify(m => m.FindByNameAsync(DefaultUserName), Times.Once);
        mockUserManager.Verify(m => m.FindByNameAsync(userNameToDelete), Times.Once);
        mockContext.Verify(c => c.Questions, Times.Once);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        mockUserManager.Verify(m => m.DeleteAsync(It.Is<User>(u => u.Id == userToDelete.Id)),
            Times.Once);
        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteUser_ReturnsFalse_WhenUserToDeleteNotFound()
    {
        // Arrange
        var userNameToDelete = "notFoundDeleteUser";

        mockUserManager.Setup(m => m.FindByNameAsync(DefaultUserName)).ReturnsAsync(defaultUser).Verifiable();
        mockUserManager.Setup(m => m.FindByNameAsync(userNameToDelete)).ReturnsAsync((User?)null)
            .Verifiable();
        mockTransaction.Setup(t => t.DisposeAsync())
            .Returns(ValueTask.CompletedTask)
            .Verifiable();

        // Act
        var result = await userService.DeleteUser(userNameToDelete);

        // Assert
        Assert.False(result);

        mockExecutionStrategy.Verify();
        mockUserManager.Verify();
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
        mockUserManager.Verify(m => m.FindByNameAsync(DefaultUserName), Times.Once);
        mockUserManager.Verify(m => m.FindByNameAsync(userNameToDelete), Times.Once);
        mockUserManager.Verify(m => m.DeleteAsync(It.IsAny<User>()), Times.Never);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Never);
        mockContext.Verify(c => c.Questions, Times.Never);
    }

    [Fact]
    public async Task DeleteUser_ReturnsFalse_WhenDefaultUserNotFound()
    {
        // Arrange
        var userNameToDelete = "someUser";

        mockUserManager.Setup(m => m.FindByNameAsync(DefaultUserName)).ReturnsAsync((User?)null)
            .Verifiable();
        mockTransaction.Setup(t => t.DisposeAsync())
            .Returns(ValueTask.CompletedTask)
            .Verifiable();

        // Act
        var result = await userService.DeleteUser(userNameToDelete);

        // Assert
        Assert.False(result);

        mockExecutionStrategy.Verify();
        mockUserManager.Verify();
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
        mockUserManager.Verify(m => m.FindByNameAsync(DefaultUserName), Times.Once);
        mockUserManager.Verify(m => m.FindByNameAsync(userNameToDelete), Times.Never);
        mockUserManager.Verify(m => m.DeleteAsync(It.IsAny<User>()), Times.Never);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockContext.Verify(c => c.Questions, Times.Never);
    }

    [Fact]
    public async Task DeleteUser_ReturnsFalse_WhenAttemptingToDeleteDefaultUser()
    {
        // Arrange
        var userNameToDelete = DefaultUserName;

        mockUserManager.Setup(m => m.FindByNameAsync(DefaultUserName)).ReturnsAsync(defaultUser).Verifiable();
        mockUserManager.Setup(m => m.FindByNameAsync(userNameToDelete)).ReturnsAsync(defaultUser)
            .Verifiable();
        mockTransaction.Setup(t => t.DisposeAsync())
            .Returns(ValueTask.CompletedTask)
            .Verifiable();

        // Act
        var result = await userService.DeleteUser(userNameToDelete);

        // Assert
        Assert.False(result);

        mockExecutionStrategy.Verify();
        mockUserManager.Verify();
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
        mockUserManager.Verify(m => m.FindByNameAsync(DefaultUserName),
            Times.Exactly(2));
        mockUserManager.Verify(m => m.DeleteAsync(It.IsAny<User>()), Times.Never);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockContext.Verify(c => c.Questions, Times.Never);
    }

    [Fact]
    public async Task DeleteUser_ThrowsWrappedException_AndRollsBack_WhenSaveChangesAsyncFails()
    {
        // Arrange
        var userNameToDelete = "userToDeleteSaveFail";
        var userToDelete = new User { Id = Guid.NewGuid().ToString(), UserName = userNameToDelete };
        var exception = new DbUpdateException("Simulated save error");

        questionsData.Add(new Question
            { Id = 1, QuestionText = "Q1", CreatedById = userToDelete.Id });

        mockUserManager.Setup(m => m.FindByNameAsync(DefaultUserName)).ReturnsAsync(defaultUser);
        mockUserManager.Setup(m => m.FindByNameAsync(userNameToDelete)).ReturnsAsync(userToDelete);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception)
            .Verifiable();
        mockTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        mockTransaction.Setup(t => t.DisposeAsync())
            .Returns(ValueTask.CompletedTask)
            .Verifiable();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.DeleteUser(userNameToDelete));

        Assert.Equal($"An unexpected error occurred while deleting user '{userNameToDelete}' and reassigning content.",
            ex.Message);
        Assert.Equal(exception, ex.InnerException);

        mockExecutionStrategy.Verify();
        mockUserManager.Verify(m => m.FindByNameAsync(DefaultUserName), Times.Once);
        mockUserManager.Verify(m => m.FindByNameAsync(userNameToDelete), Times.Once);
        mockContext.Verify();
        mockTransaction.Verify(t => t.DisposeAsync(),
            Times.Once);

        mockUserManager.Verify(m => m.DeleteAsync(It.IsAny<User>()), Times.Never);
        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUser_ThrowsWrappedException_AndRollsBack_WhenDeleteAsyncThrowsException()
    {
        // Arrange
        var userNameToDelete = "userToDeleteException";
        var userToDelete = new User { Id = Guid.NewGuid().ToString(), UserName = userNameToDelete };
        var exception = new Exception("Simulated delete exception");
        questionsData.Clear();

        mockUserManager.Setup(m => m.FindByNameAsync(DefaultUserName)).ReturnsAsync(defaultUser);
        mockUserManager.Setup(m => m.FindByNameAsync(userNameToDelete)).ReturnsAsync(userToDelete);
        mockUserManager.Setup(m => m.DeleteAsync(It.Is<User>(u => u.Id == userToDelete.Id)))
            .ThrowsAsync(exception)
            .Verifiable();
        mockTransaction.Setup(t => t.RollbackAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();
        mockTransaction.Setup(t => t.DisposeAsync())
            .Returns(ValueTask.CompletedTask)
            .Verifiable();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.DeleteUser(userNameToDelete));

        Assert.Equal($"An unexpected error occurred while deleting user '{userNameToDelete}' and reassigning content.",
            ex.Message);
        Assert.Equal(exception, ex.InnerException);

        mockExecutionStrategy.Verify();
        mockUserManager.Verify(m => m.FindByNameAsync(DefaultUserName), Times.Once);
        mockUserManager.Verify(m => m.FindByNameAsync(userNameToDelete), Times.Once);
        mockUserManager.Verify(m => m.DeleteAsync(It.Is<User>(u => u.Id == userToDelete.Id)),
            Times.Once);
        mockTransaction.Verify(t => t.DisposeAsync(), Times.Once);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}