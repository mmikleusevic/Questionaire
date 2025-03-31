using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;
using QuestionaireApi;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Services;
using Shared.Models;

namespace Tests.ServiceTests;

public class CategoryServiceTests
{
    private readonly List<Category> categoriesData;
    private readonly ICategoryService categoryService;
    private readonly Mock<QuestionaireDbContext> mockContext;
    private readonly Mock<DbSet<Category>> mockDbSet;

    public CategoryServiceTests()
    {
        var options = new DbContextOptionsBuilder<QuestionaireDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid()
                .ToString())
            .Options;

        mockContext = new Mock<QuestionaireDbContext>(options);
        categoriesData = GetTestCategories();

        mockDbSet = categoriesData.AsQueryable().BuildMockDbSet();

        mockContext.Setup(c => c.Categories).Returns(mockDbSet.Object);

        mockDbSet.Setup(m => m.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Callback<Category, CancellationToken>((cat, ct) => categoriesData.Add(cat))
            .ReturnsAsync((Category cat, CancellationToken ct) =>
                null);

        mockDbSet.Setup(m => m.Remove(It.IsAny<Category>()))
            .Callback<Category>(cat => categoriesData.Remove(cat));

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        categoryService = new CategoryService(mockContext.Object);
    }

    private List<Category> GetTestCategories()
    {
        var cat1 = new Category { Id = 1, CategoryName = "Root 1", ParentCategoryId = null };
        var cat2 = new Category { Id = 2, CategoryName = "Child 1.1", ParentCategoryId = 1 };
        var cat3 = new Category { Id = 3, CategoryName = "Child 1.2", ParentCategoryId = 1 };
        var cat4 = new Category { Id = 4, CategoryName = "Root 2", ParentCategoryId = null };
        var cat5 = new Category { Id = 5, CategoryName = "Child 2.1", ParentCategoryId = 4 };
        var cat6 = new Category { Id = 6, CategoryName = "Grandchild 1.1.1", ParentCategoryId = 2 };

        cat1.ChildCategories = new List<Category> { cat2, cat3 };
        cat2.ParentCategory = cat1;
        cat2.ChildCategories = new List<Category> { cat6 };
        cat3.ParentCategory = cat1;
        cat3.ChildCategories = new List<Category>();
        cat4.ChildCategories = new List<Category> { cat5 };
        cat5.ParentCategory = cat4;
        cat5.ChildCategories = new List<Category>();
        cat6.ParentCategory = cat2;
        cat6.ChildCategories = new List<Category>();

        return new List<Category> { cat1, cat2, cat3, cat4, cat5, cat6 };
    }

    // --- GetCategories Tests ---

    [Fact(Skip = "Cannot unit test EF.Functions.Like called internally.")]
    public async Task GetCategories_ReturnsDtoWithNestedAndFlatCategories()
    {
        // Arrange (Data is setup, async mocks are setup)

        // Act
        var result = await categoryService.GetCategories();

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.NestedCategories);
        Assert.NotNull(result.FlatCategories);
        Assert.NotEmpty(result.NestedCategories);
        Assert.NotEmpty(result.FlatCategories);
    }

    [Fact]
    public async Task GetCategories_ThrowsWrappedException_WhenDbContextAccessFails()
    {
        // Arrange
        var dbException = new Exception("Simulated DB context error");

        mockContext.Setup(c => c.Categories).Throws(dbException);

        var serviceWithThrowingContext = new CategoryService(mockContext.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            serviceWithThrowingContext.GetCategories()
        );

        Assert.Equal("An error occurred while retrieving categories.", ex.Message);
        Assert.NotNull(ex.InnerException);

        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Equal("An error occurred while retrieving categories.", ex.InnerException.Message);
        Assert.NotNull(ex.InnerException.InnerException);

        Assert.Equal(dbException, ex.InnerException.InnerException);

        mockContext.Verify(c => c.Categories, Times.AtLeastOnce());
    }

    // --- GetNestedCategories Tests ---

    [Fact]
    public async Task GetNestedCategories_ReturnsMappedAndSortedCategories()
    {
        // Act
        var result = await categoryService.GetNestedCategories();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        var root1 = result.FirstOrDefault(c => c.CategoryName == "Root 1");
        Assert.NotNull(root1);
        Assert.Equal(2, root1.ChildCategories.Count);

        var child11 = root1.ChildCategories.FirstOrDefault(c => c.CategoryName == "Child 1.1");
        Assert.NotNull(child11);
        Assert.Single(child11.ChildCategories);

        var root2 = result.FirstOrDefault(c => c.CategoryName == "Root 2");
        Assert.NotNull(root2);
        Assert.Single(root2.ChildCategories);
    }

    [Fact]
    public async Task GetNestedCategories_ReturnsEmptyList_WhenNoCategoriesExist()
    {
        // Arrange
        categoriesData.Clear();

        // Act
        var result = await categoryService.GetNestedCategories();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetNestedCategories_ThrowsWrappedException_WhenDbQueryFails()
    {
        // Arrange
        var dbException = new Exception("Simulated DB error");

        mockContext.Setup(c => c.Categories).Throws(dbException);
        var serviceWithThrowingContext = new CategoryService(mockContext.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            serviceWithThrowingContext.GetNestedCategories());
        Assert.Equal("An error occurred while retrieving categories.", ex.Message);
        Assert.Equal(dbException, ex.InnerException);
    }

    // --- GetFlatCategories Tests ---

    [Fact]
    public async Task GetFlatCategories_ThrowsWrappedException_WhenDbQueryFails()
    {
        // Arrange
        var dbException = new Exception("Simulated DB error");

        mockContext.Setup(c => c.Categories).Throws(dbException);
        var serviceWithThrowingContext = new CategoryService(mockContext.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            serviceWithThrowingContext.GetFlatCategories("test"));
        Assert.Equal("An error occurred while retrieving categories.", ex.Message);
        Assert.Equal(dbException, ex.InnerException);
    }


    // --- CreateCategory Tests ---

    [Fact]
    public async Task CreateCategory_AddsCategoryAndSavesChanges()
    {
        // Arrange
        var newCategoryDto = new CategoryExtendedDto { CategoryName = "New Root", ParentCategoryId = null };
        int initialCount = categoriesData.Count;

        // Act
        await categoryService.CreateCategory(newCategoryDto);

        // Assert
        Assert.Equal(initialCount + 1, categoriesData.Count);
        Assert.Contains(categoriesData, c => c.CategoryName == "New Root");

        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        mockDbSet.Verify(
            m => m.AddAsync(It.Is<Category>(c => c.CategoryName == newCategoryDto.CategoryName),
                It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCategory_ThrowsWrappedException_WhenAddAsyncFails()
    {
        // Arrange
        var newCategoryDto = new CategoryExtendedDto { CategoryName = "Fail Add", ParentCategoryId = null };
        var dbException = new Exception("Simulated Add error");

        mockDbSet.Setup(m => m.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>())).ThrowsAsync(dbException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            categoryService.CreateCategory(newCategoryDto));
        Assert.Equal("An error occurred while creating the category.", ex.Message);
        Assert.Equal(dbException, ex.InnerException);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateCategory_ThrowsWrappedException_WhenSaveChangesAsyncFails()
    {
        // Arrange
        var newCategoryDto = new CategoryExtendedDto { CategoryName = "Fail Save", ParentCategoryId = null };
        var dbException = new DbUpdateException("Simulated Save error", new Exception());
        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            categoryService.CreateCategory(newCategoryDto));
        Assert.Equal("An error occurred while creating the category.", ex.Message);
        Assert.Equal(dbException, ex.InnerException);

        mockDbSet.Verify(m => m.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // --- UpdateCategory Tests ---

    [Fact]
    public async Task UpdateCategory_UpdatesCategoryAndSavesChanges_WhenFound()
    {
        // Arrange
        int categoryIdToUpdate = 2;
        var updatedDto = new CategoryExtendedDto(categoryIdToUpdate)
            { CategoryName = "Updated Child 1.1", ParentCategoryId = 1 };

        // Act
        var result = await categoryService.UpdateCategory(categoryIdToUpdate, updatedDto);

        // Assert
        Assert.True(result);
        var updatedEntity = categoriesData.FirstOrDefault(c => c.Id == categoryIdToUpdate);
        Assert.NotNull(updatedEntity);
        Assert.Equal("Updated Child 1.1", updatedEntity.CategoryName);
        Assert.Equal(1, updatedEntity.ParentCategoryId);

        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCategory_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        int categoryIdToUpdate = 999;
        var updatedDto = new CategoryExtendedDto(categoryIdToUpdate) { CategoryName = "Non Existent" };

        // Act
        var result = await categoryService.UpdateCategory(categoryIdToUpdate, updatedDto);

        // Assert
        Assert.False(result);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateCategory_ThrowsWrappedException_WhenDbQueryFails()
    {
        // Arrange
        int categoryIdToUpdate = 1;
        var updatedDto = new CategoryExtendedDto(categoryIdToUpdate) { CategoryName = "Update Fail Query" };
        var dbException = new Exception("Simulated Find error");

        mockContext.Setup(c => c.Categories).Throws(dbException);
        var serviceWithThrowingContext = new CategoryService(mockContext.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            serviceWithThrowingContext.UpdateCategory(categoryIdToUpdate, updatedDto));
        Assert.Equal($"An error occurred while updating the category with ID {categoryIdToUpdate}.", ex.Message);
        Assert.Equal(dbException, ex.InnerException);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCategory_ThrowsWrappedException_WhenSaveChangesAsyncFails()
    {
        // Arrange
        int categoryIdToUpdate = 1;
        var updatedDto = new CategoryExtendedDto(categoryIdToUpdate) { CategoryName = "Update Fail Save" };
        var dbException = new DbUpdateConcurrencyException("Simulated concurrency error");

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbException);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            categoryService.UpdateCategory(categoryIdToUpdate, updatedDto));
        Assert.Equal($"An error occurred while updating the category with ID {categoryIdToUpdate}.", ex.Message);
        Assert.Equal(dbException, ex.InnerException);

        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }

    // --- DeleteCategory Tests ---

    [Fact]
    public async Task DeleteCategory_RemovesCategoryAndSavesChanges_WhenFound()
    {
        // Arrange
        int categoryIdToDelete = 3;
        int initialCount = categoriesData.Count;
        var categoryToRemove =
            categoriesData.First(c => c.Id == categoryIdToDelete);

        // Act
        var result = await categoryService.DeleteCategory(categoryIdToDelete);

        // Assert
        Assert.True(result);

        Assert.Equal(initialCount - 1, categoriesData.Count);
        Assert.DoesNotContain(categoriesData, c => c.Id == categoryIdToDelete);

        mockDbSet.Verify(m => m.Remove(It.Is<Category>(c => c.Id == categoryIdToDelete)), Times.Once);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteCategory_ReturnsFalse_WhenNotFound()
    {
        // Arrange
        int categoryIdToDelete = 999;

        // Act
        var result = await categoryService.DeleteCategory(categoryIdToDelete);

        // Assert
        Assert.False(result);
        mockDbSet.Verify(m => m.Remove(It.IsAny<Category>()), Times.Never);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCategory_ThrowsWrappedException_WhenDbQueryFails()
    {
        // Arrange
        int categoryIdToDelete = 1;
        var dbException = new Exception("Simulated Find error during delete");

        mockContext.Setup(c => c.Categories).Throws(dbException);

        var serviceWithThrowingContext = new CategoryService(mockContext.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            serviceWithThrowingContext.DeleteCategory(categoryIdToDelete)
        );

        Assert.Equal($"An error occurred while deleting the category with ID {categoryIdToDelete}.", ex.Message);
        Assert.Equal(dbException, ex.InnerException);

        mockDbSet.Verify(m => m.Remove(It.IsAny<Category>()), Times.Never);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        mockContext.Verify(c => c.Categories, Times.AtLeastOnce());
    }

    [Fact]
    public async Task DeleteCategory_ThrowsWrappedException_WhenSaveChangesAsyncFails()
    {
        // Arrange
        int categoryIdToDelete = 1;
        var dbException = new DbUpdateException("Simulated delete constraint error");

        mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(dbException);

        var categoryToRemove = categoriesData.FirstOrDefault(c => c.Id == categoryIdToDelete);
        Assert.NotNull(categoryToRemove);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            categoryService.DeleteCategory(categoryIdToDelete));
        Assert.Equal($"An error occurred while deleting the category with ID {categoryIdToDelete}.", ex.Message);
        Assert.Equal(dbException, ex.InnerException);

        mockDbSet.Verify(m => m.Remove(It.Is<Category>(c => c.Id == categoryIdToDelete)), Times.Once);
        mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}