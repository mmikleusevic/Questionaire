using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using MockQueryable.Moq;
using Moq;
using QuestionaireApi;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Services;
using Shared.Models;
using SharedStandard.Models;
using Tests.Helper;

namespace Tests.ServiceTests;

public class QuestionServiceTests
{
    private const string SuperAdminRole = "SuperAdmin";

    private readonly List<UserQuestionHistory> historyData;
    private readonly Mock<IAnswerService> mockAnswerService;
    private readonly Mock<QuestionaireDbContext> mockContext;
    private readonly Mock<IExecutionStrategy> mockExecutionStrategy;
    private readonly Mock<DbSet<UserQuestionHistory>> mockHistoryDbSet;
    private readonly Mock<IUserQuestionHistoryService> mockHistoryService;
    private readonly Mock<IQuestionCategoriesService> mockQuestionCategoriesService;
    private readonly Mock<DbSet<Question>> mockQuestionDbSet;
    private readonly Mock<IDbContextTransaction> mockTransaction;
    private readonly Mock<UserManager<User>> mockUserManager;
    private readonly List<Question> questionsData;
    private readonly IQuestionService questionService;

    public QuestionServiceTests()
    {
        var options = new DbContextOptionsBuilder<QuestionaireDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid()
                .ToString())
            .Options;

        mockContext = new Mock<QuestionaireDbContext>(options);
        mockUserManager = UserManagerMockHelper.MockUserManager<User>();
        mockHistoryService = new Mock<IUserQuestionHistoryService>();
        mockAnswerService = new Mock<IAnswerService>();
        mockQuestionCategoriesService = new Mock<IQuestionCategoriesService>();
        mockTransaction = new Mock<IDbContextTransaction>();
        mockExecutionStrategy = new Mock<IExecutionStrategy>();

        questionsData = GetTestQuestions();

        historyData = new List<UserQuestionHistory>();

        mockQuestionDbSet = questionsData.AsQueryable().BuildMockDbSet();
        mockHistoryDbSet = historyData.AsQueryable().BuildMockDbSet();


        mockContext.Setup(c => c.Questions).Returns(mockQuestionDbSet.Object);
        mockContext.Setup(c => c.UserQuestionHistory).Returns(mockHistoryDbSet.Object);

        var mockDatabaseFacade = new Mock<DatabaseFacade>(mockContext.Object);

        mockDatabaseFacade.Setup(db => db.CreateExecutionStrategy())
            .Returns(mockExecutionStrategy.Object);
        mockDatabaseFacade.Setup(db => db.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(mockTransaction.Object);
        mockContext.Setup(c => c.Database).Returns(mockDatabaseFacade.Object);

        mockExecutionStrategy
            .Setup(s => s.ExecuteAsync(
                It.IsAny<Func<Task<bool>>>(),
                It.IsAny<Func<DbContext, Func<Task<bool>>, CancellationToken, Task<bool>>>(),
                It.IsAny<Func<DbContext, Func<Task<bool>>, CancellationToken, Task<ExecutionResult<bool>>>?>(),
                It.IsAny<CancellationToken>()))
            .Returns(
                async (
                    Func<Task<bool>> state,
                    Func<DbContext, Func<Task<bool>>, CancellationToken, Task<bool>> operation,
                    Func<DbContext, Func<Task<bool>>, CancellationToken, Task<ExecutionResult<bool>>>? verifySucceeded,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await operation(mockContext.Object, state, cancellationToken);
                    return result;
                })
            .Verifiable();

        mockExecutionStrategy
            .Setup(s => s.ExecuteAsync(
                It.IsAny<Func<Task>>(),
                It.IsAny<Func<DbContext, Func<Task>, CancellationToken, Task<bool>>>(),
                It.IsAny<Func<DbContext, Func<Task>, CancellationToken, Task<ExecutionResult<bool>>>?>(),
                It.IsAny<CancellationToken>()))
            .Returns(
                async (
                    Func<Task> state,
                    Func<DbContext, Func<Task>, CancellationToken, Task<bool>> operation,
                    Func<DbContext, Func<Task>, CancellationToken, Task<ExecutionResult<bool>>>? verifySucceeded,
                    CancellationToken cancellationToken
                ) =>
                {
                    var result = await operation(mockContext.Object, state, cancellationToken);
                    return result;
                })
            .Verifiable();

        mockContext.Setup(c => c.Database).Returns(mockDatabaseFacade.Object);
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        mockQuestionDbSet.Setup(m => m.AddAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .Callback<Question, CancellationToken>((q, ct) => questionsData.Add(q))
            .ReturnsAsync((EntityEntry<Question>)null);

        questionService = new QuestionService(
            mockContext.Object,
            mockHistoryService.Object,
            mockAnswerService.Object,
            mockQuestionCategoriesService.Object,
            mockUserManager.Object
        );
    }

    // Helper to generate test questions with relations

    private List<Question> GetTestQuestions()
    {
        var q1 = new Question
        {
            Id = 1, QuestionText = "Q1 Approved?", IsApproved = true, IsDeleted = false, CreatedById = "user1",
            Difficulty = Difficulty.Easy,
            Answers = new List<Answer>
                { new Answer { Id = 1, IsCorrect = true }, new Answer { Id = 2 }, new Answer { Id = 3 } },
            QuestionCategories = new List<QuestionCategory>
                { new QuestionCategory { CategoryId = 10, Category = new Category { Id = 10, CategoryName = "Tech" } } }
        };
        var q2 = new Question
        {
            Id = 2, QuestionText = "Q2 Pending", IsApproved = false, IsDeleted = false, CreatedById = "user2",
            Difficulty = Difficulty.Easy,
            Answers = new List<Answer>
                { new Answer { Id = 4, IsCorrect = true }, new Answer { Id = 5 }, new Answer { Id = 6 } },
            QuestionCategories = new List<QuestionCategory>
            {
                new QuestionCategory { CategoryId = 20, Category = new Category { Id = 20, CategoryName = "Science" } }
            }
        };
        var q3 = new Question
        {
            Id = 3, QuestionText = "Q3 Approved Owner", IsApproved = true, IsDeleted = false,
            Difficulty = Difficulty.Easy,
            CreatedById = "user-owner",
            Answers = new List<Answer>
                { new Answer { Id = 7, IsCorrect = true }, new Answer { Id = 8 }, new Answer { Id = 9 } },
            QuestionCategories = new List<QuestionCategory>
                { new QuestionCategory { CategoryId = 10, Category = new Category { Id = 10, CategoryName = "Tech" } } }
        };
        var q4 = new Question
        {
            Id = 4, QuestionText = "Q4 Approved Different", IsApproved = true, IsDeleted = false, CreatedById = "user1",
            Difficulty = Difficulty.Easy,
            Answers = new List<Answer>
                { new Answer { Id = 10, IsCorrect = true }, new Answer { Id = 11 }, new Answer { Id = 12 } },
            QuestionCategories = new List<QuestionCategory>
            {
                new QuestionCategory { CategoryId = 20, Category = new Category { Id = 20, CategoryName = "Science" } }
            }
        };
        var q5NeedsApproval = new Question
        {
            Id = 5, QuestionText = "Q5 Needs Approval", IsApproved = false, IsDeleted = false, CreatedById = "user1",
            Difficulty = Difficulty.Easy,
            Answers = new List<Answer>
                { new Answer { Id = 13, IsCorrect = true }, new Answer { Id = 14 }, new Answer { Id = 15 } },
            QuestionCategories = new List<QuestionCategory>
                { new QuestionCategory { CategoryId = 10, Category = new Category { Id = 10, CategoryName = "Tech" } } }
        };
        var q6InvalidAnswers = new Question
        {
            Id = 6, QuestionText = "Q6 Invalid Answers", IsApproved = false, IsDeleted = false, CreatedById = "user1",
            Difficulty = Difficulty.Easy,
            Answers = new List<Answer>
                { new Answer { Id = 16, IsCorrect = true }, new Answer { Id = 17 } },
            QuestionCategories = new List<QuestionCategory>
                { new QuestionCategory { CategoryId = 10, Category = new Category { Id = 10, CategoryName = "Tech" } } }
        };
        var q7NoCategory = new Question
        {
            Id = 7, QuestionText = "Q7 No Category", IsApproved = false, IsDeleted = false, CreatedById = "user1",
            Difficulty = Difficulty.Easy,
            Answers = new List<Answer>
                { new Answer { Id = 18, IsCorrect = true }, new Answer { Id = 19 }, new Answer { Id = 20 } },
            QuestionCategories = new List<QuestionCategory>()
        };
        var q8Deleted = new Question
        {
            Id = 8, QuestionText = "Q8 Deleted", IsApproved = true, IsDeleted = true, CreatedById = "user1",
            Difficulty = Difficulty.Easy,
            Answers = new List<Answer>
                { new Answer { Id = 21, IsCorrect = true }, new Answer { Id = 22 }, new Answer { Id = 23 } },
            QuestionCategories = new List<QuestionCategory>
                { new QuestionCategory { CategoryId = 10, Category = new Category { Id = 10, CategoryName = "Tech" } } }
        };

        return new List<Question> { q1, q2, q3, q4, q5NeedsApproval, q6InvalidAnswers, q7NoCategory, q8Deleted };
    }

    // Helper to create ClaimsPrincipal

    private ClaimsPrincipal CreateClaimsPrincipal(string userId, params string[] roles)
    {
        var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    // --- GetQuestions Tests ---

    [Fact(Skip = "EF.Functions.Like cannot be translated by InMemory provider.")]
    public async Task GetQuestions_FiltersBySearchQuery_UsingLike()
    {
        // Arrange
        var user = CreateClaimsPrincipal("user1");
        var dbUser = new User { Id = "user1" };
        var request = new QuestionsRequestDto
            { FetchApprovedQuestions = true, PageNumber = 1, PageSize = 10, SearchQuery = "Approved" };
        mockUserManager.Setup(um => um.GetUserAsync(user)).ReturnsAsync(dbUser);

        // Act
        var result = await questionService.GetQuestions(request, user);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetQuestions_ReturnsApprovedQuestions_WhenRequested()
    {
        // Arrange
        var user = CreateClaimsPrincipal("user1");
        var dbUser = new User { Id = "user1" };
        var request = new QuestionsRequestDto { FetchApprovedQuestions = true, PageNumber = 1, PageSize = 10 };
        mockUserManager.Setup(um => um.GetUserAsync(user)).ReturnsAsync(dbUser);

        // Act
        var result = await questionService.GetQuestions(request, user);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.Equal(3, result.TotalCount);
        Assert.All(result.Items, item => Assert.Contains(item.Id, new[] { 1, 3, 4 }));
        Assert.All(result.Items, item =>
        {
            Assert.NotNull(item.Answers);
            Assert.True(item.Answers.Count > 0);
        });
        Assert.All(result.Items, item =>
        {
            Assert.NotNull(item.Categories);
            Assert.True(item.Categories.Count > 0);
        });
    }

    [Fact]
    public async Task GetQuestions_ReturnsPendingQuestions_WhenRequested()
    {
        // Arrange
        var user = CreateClaimsPrincipal("user2");
        var dbUser = new User { Id = "user2" };
        var request = new QuestionsRequestDto { FetchApprovedQuestions = false, PageNumber = 1, PageSize = 10 };
        mockUserManager.Setup(um => um.GetUserAsync(user)).ReturnsAsync(dbUser);

        // Act
        var result = await questionService.GetQuestions(request, user);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result.Items);
        Assert.Equal(4, result.TotalCount);
        Assert.All(result.Items, item => Assert.Contains(item.Id, new[] { 2, 5, 6, 7 }));
    }

    [Fact]
    public async Task GetQuestions_FiltersByOwnQuestions_WhenRequested()
    {
        // Arrange
        var userId = "user2";
        var user = CreateClaimsPrincipal(userId);
        var dbUser = new User { Id = userId };
        var request = new QuestionsRequestDto
        {
            OnlyMyQuestions = true, FetchApprovedQuestions = false, PageNumber = 1, PageSize = 10
        };
        mockUserManager.Setup(um => um.GetUserAsync(user)).ReturnsAsync(dbUser);

        // Act
        var result = await questionService.GetQuestions(request, user);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(2, result.Items[0].Id);
    }

    [Fact]
    public async Task GetQuestions_ReturnsCorrectPagination()
    {
        // Arrange
        var user = CreateClaimsPrincipal("user1");
        var dbUser = new User { Id = "user1" };
        var request = new QuestionsRequestDto { FetchApprovedQuestions = true, PageNumber = 2, PageSize = 1 };
        mockUserManager.Setup(um => um.GetUserAsync(user)).ReturnsAsync(dbUser);

        // Act
        var result = await questionService.GetQuestions(request, user);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(3, result.Items[0].Id);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(1, request.PageSize);
        Assert.Equal(2, request.PageNumber);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task GetQuestions_ReturnsEmpty_WhenUserNotFound()
    {
        // Arrange
        var user = CreateClaimsPrincipal("unknown");
        var request = new QuestionsRequestDto { PageNumber = 1, PageSize = 10 };
        mockUserManager.Setup(um => um.GetUserAsync(user)).ReturnsAsync((User)null);

        // Act
        var result = await questionService.GetQuestions(request, user);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetQuestions_ThrowsWrappedException_WhenDbFails()
    {
        // Arrange
        var user = CreateClaimsPrincipal("user1");
        var dbUser = new User { Id = "user1" };
        var request = new QuestionsRequestDto { PageNumber = 1, PageSize = 10 };
        var dbException = new Exception("DB Error");
        mockUserManager.Setup(um => um.GetUserAsync(user)).ReturnsAsync(dbUser);
        mockContext.Setup(c => c.Questions).Throws(dbException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            questionService.GetQuestions(request, user));
        Assert.Equal("An error occurred while retrieving the list of questions.", ex.Message);
        Assert.Equal(dbException, ex.InnerException);
    }

    // --- GetRandomUniqueQuestions Tests ---

    [Fact(Skip = "EF.Functions.Like cannot be translated by InMemory provider.")]
    public async Task GetRandomUniqueQuestions_FetchesQuestions_WhenEnoughFoundInitially()
    {
        // Arrange
        var userId = "testUserForRandom";
        var request = new UniqueQuestionsRequestDto
        {
            UserId = userId, NumberOfQuestions = 2, CategoryIds = [10], IsSingleAnswerMode = false,
            Difficulties = [Difficulty.Easy]
        };

        var availableQuestions = questionsData
            .Where(q => q.IsApproved && !q.IsDeleted && q.QuestionCategories.Any(qc => qc.CategoryId == 10))
            .ToList();

        // Act
        var result = await questionService.GetRandomUniqueQuestions(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.NumberOfQuestions, result.Count);
        mockHistoryService.Verify(h => h.ResetUserQuestionHistoryForCriteria(It.IsAny<string>(),
                It.IsAny<List<int>>(), It.IsAny<List<Difficulty>>()),
            Times.Never);
    }

    [Fact(Skip = "EF.Functions.Like cannot be translated by InMemory provider.")]
    public async Task GetRandomUniqueQuestions_ResetsHistoryAndFetchesMore_WhenInsufficientFoundInitially()
    {
        // Arrange
        var userId = "testUserForRandomReset";
        var request = new UniqueQuestionsRequestDto
        {
            UserId = userId, NumberOfQuestions = 3, CategoryIds = [10], IsSingleAnswerMode = false,
            Difficulties = [Difficulty.Easy]
        };

        // Act
        var result = await questionService.GetRandomUniqueQuestions(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Id == 1);
        Assert.Contains(result, r => r.Id == 3);
        mockHistoryService.Verify(
            h => h.ResetUserQuestionHistoryForCriteria(request.UserId, request.CategoryIds, request.Difficulties),
            Times.Once);
    }

    // --- ApproveQuestion Tests ---

    [Fact]
    public async Task ApproveQuestion_ApprovesValidQuestionAndSaves_WhenUserIsSuperAdmin()
    {
        // Arrange
        var userId = "superadmin1";
        var user = CreateClaimsPrincipal(userId, SuperAdminRole);
        var questionToApprove = questionsData.First(q => q.Id == 5);
        questionToApprove.CreatedById = "anotherUser";
        Assert.False(questionToApprove.IsApproved);
        mockUserManager.Setup(um => um.GetUserId(user)).Returns(userId);

        // Act
        var result = await questionService.ApproveQuestion(questionToApprove.Id, user);

        // Assert
        Assert.True(result);
        Assert.True(questionToApprove.IsApproved);
        Assert.Equal(userId, questionToApprove.ApprovedById);
        Assert.NotNull(questionToApprove.ApprovedAt);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveQuestion_ReturnsFalse_WhenUserIsUser()
    {
        // Arrange
        var userId = "notauser";
        var user = CreateClaimsPrincipal(userId, "User");
        int questionId = 5;

        // Act & Assert
        bool result = await questionService.ApproveQuestion(questionId, user);

        Assert.False(result);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveQuestion_ThrowsUnauthorized_WhenUserIdNotFound_And_UserHasSuperAdminRole()
    {
        // Arrange
        var user = CreateClaimsPrincipal("some-dummy-id", SuperAdminRole);
        int questionId = 5;
        mockUserManager.Setup(um => um.GetUserId(user)).Returns((string?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            questionService.ApproveQuestion(questionId, user));

        Assert.Equal("The user is not authorized.", ex?.InnerException?.Message);

        mockUserManager.Verify(um => um.GetUserId(user), Times.Once);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveQuestion_ReturnsFalse_WhenUserApprovesOwnQuestion_And_IsNotSuperAdmin()
    {
        // Arrange
        var userId = "creator_user";
        var user = CreateClaimsPrincipal(userId, "User");
        var questionToApprove = questionsData.First(q => q.Id == 5);
        questionToApprove.CreatedById = userId;
        Assert.False(questionToApprove.IsApproved);
        mockUserManager.Setup(um => um.GetUserId(user)).Returns(userId);

        // Act
        var result = await questionService.ApproveQuestion(questionToApprove.Id, user);

        // Assert
        Assert.False(result);
        Assert.False(questionToApprove.IsApproved);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveQuestion_ApprovesOwnQuestion_WhenUserIsSuperAdmin()
    {
        // Arrange
        var userId = "superadmin_creator";
        var user = CreateClaimsPrincipal(userId, SuperAdminRole);
        var questionToApprove = questionsData.First(q => q.Id == 5);
        questionToApprove.CreatedById = userId;
        Assert.False(questionToApprove.IsApproved);
        mockUserManager.Setup(um => um.GetUserId(user)).Returns(userId);

        // Act
        var result = await questionService.ApproveQuestion(questionToApprove.Id, user);

        // Assert
        Assert.True(result);
        Assert.True(questionToApprove.IsApproved);
        Assert.Equal(userId, questionToApprove.ApprovedById);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ApproveQuestion_ReturnsFalse_WhenQuestionAlreadyApproved()
    {
        // Arrange
        var userId = "superadmin2";
        var user = CreateClaimsPrincipal(userId, SuperAdminRole);
        var questionAlreadyApproved = questionsData.First(q => q.Id == 1);
        Assert.True(questionAlreadyApproved.IsApproved);
        var originalApprovedAt = questionAlreadyApproved.ApprovedAt;
        var originalApprovedById = questionAlreadyApproved.ApprovedById;
        mockUserManager.Setup(um => um.GetUserId(user)).Returns(userId);

        // Act
        var result = await questionService.ApproveQuestion(questionAlreadyApproved.Id, user);

        // Assert
        Assert.False(result);
        Assert.Equal(originalApprovedAt, questionAlreadyApproved.ApprovedAt);
        Assert.Equal(originalApprovedById, questionAlreadyApproved.ApprovedById);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ApproveQuestion_ReturnsFalse_WhenQuestionNotFound()
    {
        // Arrange
        var user = CreateClaimsPrincipal("admin1");
        int nonExistentId = 999;

        // Act
        var result = await questionService.ApproveQuestion(nonExistentId, user);

        // Assert
        Assert.False(result);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(6)]
    [InlineData(7)]
    public async Task ApproveQuestion_ThrowsInvalidOperation_WhenQuestionIsInvalid_AndUserIsSuperAdmin(
        int invalidQuestionId)
    {
        // Arrange
        var user = CreateClaimsPrincipal("superadmin3", SuperAdminRole);
        mockUserManager.Setup(um => um.GetUserId(user)).Returns("superadmin3");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            questionService.ApproveQuestion(invalidQuestionId, user));

        Assert.Equal("An error occurred while approving the question.", ex.Message);
        Assert.NotNull(ex.InnerException);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Equal(
            "Invalid question: must have exactly 3 answers, 2 incorrect answers, 1 correct answer and at least one category.",
            ex.InnerException.Message);
    }

    // --- CreateQuestion Tests ---

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public async Task CreateQuestion_ThrowsInvalidOperation_WhenAnswerCountIsNotThree(int answerCount)
    {
        // Arrange
        var userId = "creator_invalid_answers";
        var user = CreateClaimsPrincipal(userId);

        mockUserManager.Setup(um => um.GetUserId(user)).Returns(userId);
        var answers = Enumerable.Range(1, answerCount)
            .Select(i => new AnswerExtendedDto { IsCorrect = i == 1 })
            .ToList();
        var invalidDto = new QuestionExtendedDto
        {
            QuestionText = "Invalid Answer Count",
            Answers = answers,
            Categories = new List<CategoryExtendedDto> { new CategoryExtendedDto(10) }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            questionService.CreateQuestion(invalidDto, user));

        Assert.NotNull(ex.InnerException);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains(
            "Invalid question: must have exactly 3 answers, 2 incorrect answers, 1 correct answer and at least one category.",
            ex.InnerException.Message);

        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateQuestion_ThrowsInvalidOperation_WhenNoCorrectAnswer()
    {
        // Arrange
        var userId = "creator_no_correct";
        var user = CreateClaimsPrincipal(userId);
        mockUserManager.Setup(um => um.GetUserId(user)).Returns(userId);
        var invalidDto = new QuestionExtendedDto
        {
            QuestionText = "No Correct Answer",
            Answers = new List<AnswerExtendedDto>
                { new AnswerExtendedDto(), new AnswerExtendedDto(), new AnswerExtendedDto() },
            Categories = new List<CategoryExtendedDto> { new CategoryExtendedDto(10) }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            questionService.CreateQuestion(invalidDto, user));
        Assert.NotNull(ex.InnerException);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains("1 correct answer", ex.InnerException.Message);
    }

    [Fact]
    public async Task CreateQuestion_ThrowsInvalidOperation_WhenNoCategories()
    {
        // Arrange
        var userId = "creator_no_cats";
        var user = CreateClaimsPrincipal(userId);
        mockUserManager.Setup(um => um.GetUserId(user)).Returns(userId);
        var invalidDto = new QuestionExtendedDto
        {
            QuestionText = "No Categories",
            Answers = new List<AnswerExtendedDto>
                { new AnswerExtendedDto { IsCorrect = true }, new AnswerExtendedDto(), new AnswerExtendedDto() },
            Categories = new List<CategoryExtendedDto>()
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            questionService.CreateQuestion(invalidDto, user));
        Assert.NotNull(ex.InnerException);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains("at least one category", ex.InnerException.Message);
    }

    [Fact]
    public async Task CreateQuestion_CreatesQuestionAndRelatedEntities_DoesNotApprove_WhenUserIsNotSuperAdmin()
    {
        // Arrange
        var userId = "creator1_non_admin";
        var user = CreateClaimsPrincipal(userId, "User");
        var newQuestionDto = new QuestionExtendedDto
        {
            QuestionText = "New Q Create NonAdmin?",
            Answers = new List<AnswerExtendedDto>
                { new AnswerExtendedDto { IsCorrect = true }, new AnswerExtendedDto(), new AnswerExtendedDto() },
            Categories = new List<CategoryExtendedDto> { new CategoryExtendedDto(10) }
        };
        int questionIdAfterSave = 99;
        Question addedQuestion = null;

        mockUserManager.Setup(um => um.GetUserId(user)).Returns(userId);
        mockQuestionDbSet.Setup(m => m.AddAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .Callback<Question, CancellationToken>((q, ct) =>
            {
                q.Id = questionIdAfterSave;
                addedQuestion = q;
                questionsData.Add(q);
            })
            .ReturnsAsync((EntityEntry<Question>)null);

        // Act
        await questionService.CreateQuestion(newQuestionDto, user);

        // Assert
        Assert.NotNull(addedQuestion);
        Assert.False(addedQuestion.IsApproved);
        Assert.Null(addedQuestion.ApprovedById);

        mockContext.Verify(c => c.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);

        mockQuestionDbSet.Verify(
            db => db.AddAsync(
                It.Is<Question>(q => q.QuestionText == newQuestionDto.QuestionText && q.CreatedById == userId),
                It.IsAny<CancellationToken>()), Times.Once);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));

        mockAnswerService.Verify(a => a.CreateQuestionAnswers(questionIdAfterSave, newQuestionDto.Answers), Times.Once);
        mockQuestionCategoriesService.Verify(
            qc => qc.CreateQuestionCategories(questionIdAfterSave, newQuestionDto.Categories), Times.Once);
    }

    [Fact]
    public async Task CreateQuestion_CreatesQuestionAndAutoApproves_WhenUserIsSuperAdmin()
    {
        // Arrange
        var userId = "superadmin_creator";
        var user = CreateClaimsPrincipal(userId, SuperAdminRole);
        var newQuestionDto = new QuestionExtendedDto
        {
            QuestionText = "New Q Create SuperAdmin?",
            Answers = new List<AnswerExtendedDto>
            {
                new AnswerExtendedDto { AnswerText = "Correct", IsCorrect = true },
                new AnswerExtendedDto { AnswerText = "Wrong A" },
                new AnswerExtendedDto { AnswerText = "Wrong B" }
            },
            Categories = new List<CategoryExtendedDto> { new CategoryExtendedDto(10) { CategoryName = "Tech" } }
        };
        int questionIdAfterSave = 5;
        Question addedQuestion = null;

        mockUserManager.Setup(um => um.GetUserId(user)).Returns(userId);

        mockQuestionDbSet.Setup(m => m.AddAsync(It.IsAny<Question>(), It.IsAny<CancellationToken>()))
            .Callback<Question, CancellationToken>((q, ct) =>
            {
                q.Id = questionIdAfterSave;
                addedQuestion = q;
                var existing = questionsData.FirstOrDefault(x => x.Id == q.Id);
                if (existing == null) questionsData.Add(q);
            })
            .ReturnsAsync((EntityEntry<Question>)null);

        mockAnswerService.Setup(a => a.CreateQuestionAnswers(questionIdAfterSave, newQuestionDto.Answers))
            .Callback(() =>
            {
                if (addedQuestion != null)
                {
                    addedQuestion.Answers = newQuestionDto.Answers.Select((aDto, index) => new Answer
                    {
                        Id = 50 + index,
                        QuestionId = questionIdAfterSave,
                        AnswerText = aDto.AnswerText,
                        IsCorrect = aDto.IsCorrect,
                        Question = addedQuestion
                    }).ToList();
                }
            })
            .Returns(Task.CompletedTask);

        mockQuestionCategoriesService
            .Setup(qc => qc.CreateQuestionCategories(questionIdAfterSave, newQuestionDto.Categories))
            .Callback(() =>
            {
                if (addedQuestion != null)
                {
                    addedQuestion.IsApproved = true;
                    addedQuestion.ApprovedById = userId;
                    addedQuestion.ApprovedAt = DateTime.Now;
                    addedQuestion.QuestionCategories = newQuestionDto.Categories.Select((cDto, index) =>
                        new QuestionCategory
                        {
                            QuestionId = questionIdAfterSave,
                            CategoryId = cDto.Id,
                            Question = addedQuestion
                        }).ToList();
                }
            })
            .Returns(Task.CompletedTask);

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        await questionService.CreateQuestion(newQuestionDto, user);

        // Assert
        Assert.NotNull(addedQuestion);
        Assert.True(addedQuestion.IsApproved, "Question should be approved by SuperAdmin");
        Assert.Equal(userId, addedQuestion.ApprovedById);
        Assert.NotNull(addedQuestion.ApprovedAt);

        mockContext.Verify(c => c.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);

        mockQuestionDbSet.Verify(
            db => db.AddAsync(
                It.Is<Question>(q => q.QuestionText == newQuestionDto.QuestionText && q.CreatedById == userId),
                It.IsAny<CancellationToken>()), Times.Once);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
        mockAnswerService.Verify(a => a.CreateQuestionAnswers(questionIdAfterSave, newQuestionDto.Answers), Times.Once);
        mockQuestionCategoriesService.Verify(
            qc => qc.CreateQuestionCategories(questionIdAfterSave, newQuestionDto.Categories), Times.Once);
    }

    // --- UpdateQuestion Tests ---

    [Theory]
    [InlineData(2)]
    [InlineData(4)]
    public async Task UpdateQuestion_ThrowsInvalidOperation_WhenAnswerCountIsNotThree(int answerCount)
    {
        // Arrange
        var userId = "updater_invalid_answers";
        var user = CreateClaimsPrincipal(userId);
        int questionIdToUpdate = 1;
        mockUserManager.Setup(um => um.GetUserId(user)).Returns(userId);
        var answers = Enumerable.Range(1, answerCount)
            .Select(i => new AnswerExtendedDto { IsCorrect = i == 1 })
            .ToList();
        var invalidDto = new QuestionExtendedDto(questionIdToUpdate)
        {
            QuestionText = "Invalid Answer Count Update",
            Answers = answers,
            Categories = new List<CategoryExtendedDto> { new CategoryExtendedDto(10) }
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            questionService.UpdateQuestion(questionIdToUpdate, invalidDto, user));

        Assert.NotNull(ex.InnerException);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains("Invalid question: must have exactly 3 answers", ex.InnerException.Message);

        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateQuestion_ThrowsUnauthorized_WhenUserIdNotFound()
    {
        // Arrange
        var user = CreateClaimsPrincipal("dummy-id");
        int questionIdToUpdate = 1;
        var validDto = new QuestionExtendedDto(questionIdToUpdate)
        {
            QuestionText = "Question Text",
            Answers = new List<AnswerExtendedDto>
            {
                new AnswerExtendedDto
                {
                    IsCorrect = true,
                    AnswerText = "Answer Text"
                },
                new AnswerExtendedDto
                {
                    IsCorrect = false,
                    AnswerText = "Answer Text"
                },
                new AnswerExtendedDto
                {
                    IsCorrect = false,
                    AnswerText = "Answer Text"
                }
            },
            Categories = new List<CategoryExtendedDto>
            {
                new CategoryExtendedDto(10)
                {
                    CategoryName = "Category Name"
                }
            },
            Difficulty = Difficulty.Easy
        };

        mockUserManager.Setup(um => um.GetUserId(user)).Returns((string?)null);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            questionService.UpdateQuestion(questionIdToUpdate, validDto, user));
        Assert.Equal($"An error occurred while updating the question with ID {questionIdToUpdate}.", ex.Message);
        Assert.IsType<UnauthorizedAccessException>(ex.InnerException);
        Assert.Equal("The user is not authorized", ex.InnerException.Message);
    }

    [Fact]
    public async Task UpdateQuestion_ThrowsException_WhenAnswerServiceFails()
    {
        // Arrange
        var userId = "updater_fail_answer";
        var user = CreateClaimsPrincipal(userId);
        int questionIdToUpdate = 1;
        var updateDto = new QuestionExtendedDto(questionIdToUpdate)
        {
            QuestionText = "Question Text",
            Answers = new List<AnswerExtendedDto>
            {
                new AnswerExtendedDto
                {
                    IsCorrect = true,
                    AnswerText = "Answer Text"
                },
                new AnswerExtendedDto
                {
                    IsCorrect = false,
                    AnswerText = "Answer Text"
                },
                new AnswerExtendedDto
                {
                    IsCorrect = false,
                    AnswerText = "Answer Text"
                }
            },
            Categories = new List<CategoryExtendedDto>
            {
                new CategoryExtendedDto(10)
                {
                    CategoryName = "Category Name"
                }
            },
            Difficulty = Difficulty.Easy
        };
        var serviceException = new Exception("Answer service update failure");
        mockUserManager.Setup(um => um.GetUserId(user)).Returns(userId);
        mockAnswerService.Setup(a =>
                a.UpdateQuestionAnswers(questionIdToUpdate, It.IsAny<ICollection<Answer>>(), updateDto.Answers))
            .ThrowsAsync(serviceException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            questionService.UpdateQuestion(questionIdToUpdate, updateDto, user));
        Assert.Equal($"An error occurred while updating the question with ID {questionIdToUpdate}.", ex.Message);
        Assert.Equal(serviceException, ex.InnerException);

        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateQuestion_ThrowsException_WhenCategoryServiceFails()
    {
        // Arrange
        var userId = "updater_fail_category";
        var user = CreateClaimsPrincipal(userId);
        int questionIdToUpdate = 1;
        var updateDto = new QuestionExtendedDto(questionIdToUpdate)
        {
            QuestionText = "Question Text",
            Answers = new List<AnswerExtendedDto>
            {
                new AnswerExtendedDto
                {
                    IsCorrect = true,
                    AnswerText = "Answer Text"
                },
                new AnswerExtendedDto
                {
                    IsCorrect = false,
                    AnswerText = "Answer Text"
                },
                new AnswerExtendedDto
                {
                    IsCorrect = false,
                    AnswerText = "Answer Text"
                }
            },
            Categories = new List<CategoryExtendedDto>
            {
                new CategoryExtendedDto(10)
                {
                    CategoryName = "Category Name"
                }
            },
            Difficulty = Difficulty.Easy
        };
        var serviceException = new Exception("Category service update failure");

        mockUserManager.Setup(um => um.GetUserId(user)).Returns(userId);
        mockAnswerService.Setup(a =>
                a.UpdateQuestionAnswers(It.IsAny<int>(), It.IsAny<ICollection<Answer>>(),
                    It.IsAny<List<AnswerExtendedDto>>()))
            .Returns(Task.CompletedTask);
        mockQuestionCategoriesService.Setup(qc =>
                qc.UpdateQuestionCategories(questionIdToUpdate, It.IsAny<ICollection<QuestionCategory>>(),
                    updateDto.Categories))
            .ThrowsAsync(serviceException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            questionService.UpdateQuestion(questionIdToUpdate, updateDto, user));
        Assert.Equal($"An error occurred while updating the question with ID {questionIdToUpdate}.", ex.Message);
        Assert.Equal(serviceException, ex.InnerException);

        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateQuestion_UpdatesQuestionAndRelatedEntities_WithinTransaction()
    {
        // Arrange
        var userId = "updater1";
        var user = CreateClaimsPrincipal(userId);
        int questionIdToUpdate = 1;
        var updatedDto = new QuestionExtendedDto(questionIdToUpdate)
        {
            QuestionText = "Updated Q1 Text",
            Answers = new List<AnswerExtendedDto>
                { new AnswerExtendedDto(1) { IsCorrect = true }, new AnswerExtendedDto(2), new AnswerExtendedDto(3) },
            Categories = new List<CategoryExtendedDto> { new CategoryExtendedDto(10), new CategoryExtendedDto(20) }
        };
        var existingQuestion = questionsData.First(q => q.Id == questionIdToUpdate);

        // Act
        var result = await questionService.UpdateQuestion(questionIdToUpdate, updatedDto, user);

        // Assert
        Assert.True(result);

        mockContext.Verify(c => c.Database.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()), Times.Never);

        Assert.Equal(updatedDto.QuestionText, existingQuestion.QuestionText);
        Assert.Equal(userId, existingQuestion.LastUpdatedById);
        Assert.NotNull(existingQuestion.LastUpdatedAt);

        mockAnswerService.Verify(
            a => a.UpdateQuestionAnswers(questionIdToUpdate, It.IsAny<ICollection<Answer>>(), updatedDto.Answers),
            Times.Once);
        mockQuestionCategoriesService.Verify(
            qc => qc.UpdateQuestionCategories(questionIdToUpdate, It.IsAny<ICollection<QuestionCategory>>(),
                updatedDto.Categories), Times.Once);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateQuestion_ReturnsFalseAndRollsback_WhenQuestionNotFound()
    {
        // Arrange
        var userId = "updater2";
        var user = CreateClaimsPrincipal(userId);
        int nonExistentId = 999;
        var updatedDto = new QuestionExtendedDto(nonExistentId)
        {
            QuestionText = "Non Existent Update",
            Answers = new List<AnswerExtendedDto>
                { new AnswerExtendedDto { IsCorrect = true }, new AnswerExtendedDto(), new AnswerExtendedDto() },
            Categories = new List<CategoryExtendedDto> { new CategoryExtendedDto(10) }
        };

        // Act
        var result = await questionService.UpdateQuestion(nonExistentId, updatedDto, user);

        // Assert
        Assert.False(result);
        mockTransaction.Verify(t => t.RollbackAsync(It.IsAny<CancellationToken>()),
            Times.Once);
        mockTransaction.Verify(t => t.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- DeleteQuestion Tests ---

    [Theory]
    [InlineData("user-owner", 3, true)]
    [InlineData("admin", 3, true)]
    [InlineData("super", 3, true)]
    [InlineData("other-user", 3, false)]
    public async Task DeleteQuestion_DeletesOrDeniesBasedOnRoleAndOwnership(string roleOrOwnerId, int questionId,
        bool shouldSucceed)
    {
        // Arrange
        ClaimsPrincipal user;
        if (roleOrOwnerId == "user-owner")
        {
            user = CreateClaimsPrincipal("user-owner", "User");
        }
        else if (roleOrOwnerId == "admin")
        {
            user = CreateClaimsPrincipal("admin-id", "Admin");
        }
        else if (roleOrOwnerId == "super")
        {
            user = CreateClaimsPrincipal("super-id", "SuperAdmin");
        }
        else
        {
            user = CreateClaimsPrincipal("other-user", "User");
        }

        var questionToDelete = questionsData.First(q => q.Id == questionId);
        Assert.False(questionToDelete.IsDeleted);

        // Act
        var result = await questionService.DeleteQuestion(questionId, user);

        // Assert
        Assert.Equal(shouldSucceed, result);
        if (shouldSucceed)
        {
            Assert.True(questionToDelete.IsDeleted);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        else
        {
            Assert.False(questionToDelete.IsDeleted);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }

    [Fact]
    public async Task DeleteQuestion_ReturnsFalse_WhenQuestionNotFound()
    {
        // Arrange
        var user = CreateClaimsPrincipal("admin", "Admin");
        int nonExistentId = 999;

        // Act
        var result = await questionService.DeleteQuestion(nonExistentId, user);

        // Assert
        Assert.False(result);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteQuestion_ReturnsFalse_WhenUserUnauthorized()
    {
        // Arrange

        var user = new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>(), "TestAuth"));
        int questionId = 3;
        mockUserManager.Setup(um => um.GetUserId(user)).Returns((string)null);

        // Act
        var result = await questionService.DeleteQuestion(questionId, user);

        // Assert
        Assert.False(result);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteQuestion_ThrowsWrappedException_WhenDbFails()
    {
        // Arrange
        var user = CreateClaimsPrincipal("admin", "Admin");
        int questionId = 3;
        var dbException = new Exception("DB Error on delete save");
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(dbException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            questionService.DeleteQuestion(questionId, user));
        Assert.Equal($"An error occurred while deleting the question with ID {questionId}.", ex.Message);
        Assert.Equal(dbException, ex.InnerException);
    }
}