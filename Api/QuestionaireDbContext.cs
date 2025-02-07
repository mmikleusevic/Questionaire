using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Models;

namespace QuestionaireApi;

public class QuestionaireDbContext(DbContextOptions<QuestionaireDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<QuestionCategory> QuestionCategories => Set<QuestionCategory>();
    public DbSet<UserQuestionHistory> UserQuestionHistory => Set<UserQuestionHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuestionCategory>()
            .HasKey(qc => new { qc.QuestionId, qc.CategoryId });

        modelBuilder.Entity<QuestionCategory>()
            .HasOne(qc => qc.Question)
            .WithMany(q => q.QuestionCategories)
            .HasForeignKey(qc => qc.QuestionId);

        modelBuilder.Entity<QuestionCategory>()
            .HasOne(qc => qc.Category)
            .WithMany(c => c.QuestionCategories)
            .HasForeignKey(qc => qc.CategoryId);
        
        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.ChildCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Answer>()
            .HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, CategoryName = "General Knowledge" },
            new Category { Id = 2, CategoryName = "Science" },
            new Category { Id = 3, CategoryName = "Physics", ParentCategoryId = 2 },
            new Category { Id = 4, CategoryName = "Biology", ParentCategoryId = 2 },
            new Category { Id = 5, CategoryName = "History" },
            new Category { Id = 6, CategoryName = "Ancient History", ParentCategoryId = 5 },
            new Category { Id = 7, CategoryName = "Modern History", ParentCategoryId = 5 },
            new Category { Id = 8, CategoryName = "Geography" },
            new Category { Id = 9, CategoryName = "World Geography", ParentCategoryId = 8 },
            new Category { Id = 10, CategoryName = "Music" },
            new Category { Id = 11, CategoryName = "Sports" }
        );
    }
}