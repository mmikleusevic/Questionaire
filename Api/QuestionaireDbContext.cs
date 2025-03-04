using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Models;
using QuestionaireApi.Models.Database;

namespace QuestionaireApi;

public class QuestionaireDbContext(DbContextOptions options) : IdentityDbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<QuestionCategory> QuestionCategories => Set<QuestionCategory>();
    public DbSet<UserQuestionHistory> UserQuestionHistory => Set<UserQuestionHistory>();
    public DbSet<PendingQuestion> PendingQuestions => Set<PendingQuestion>();
    public DbSet<PendingAnswer> PendingAnswers => Set<PendingAnswer>();
    public DbSet<PendingQuestionCategory> PendingQuestionCategories => Set<PendingQuestionCategory>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<QuestionCategory>()
            .HasKey(qc => new { qc.QuestionId, qc.CategoryId });

        modelBuilder.Entity<QuestionCategory>()
            .HasOne(qc => qc.Question)
            .WithMany(q => q.QuestionCategories)
            .HasForeignKey(qc => qc.QuestionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<QuestionCategory>()
            .HasOne(qc => qc.Category)
            .WithMany(c => c.QuestionCategories)
            .HasForeignKey(qc => qc.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); 
        
        modelBuilder.Entity<Category>()
            .HasOne(c => c.ParentCategory)
            .WithMany(c => c.ChildCategories)
            .HasForeignKey(c => c.ParentCategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        modelBuilder.Entity<Answer>()
            .HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<PendingAnswer>()
            .HasOne(a => a.PendingQuestion)
            .WithMany(q => q.PendingAnswers)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<PendingQuestionCategory>()
            .HasKey(qc => new { qc.PendingQuestionId, qc.CategoryId });
        
        modelBuilder.Entity<PendingQuestionCategory>()
            .HasOne(qc => qc.PendingQuestion)
            .WithMany(q => q.PendingQuestionCategories)
            .HasForeignKey(qc => qc.PendingQuestionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<PendingQuestionCategory>()
            .HasOne(qc => qc.Category)
            .WithMany(c => c.PendingQuestionCategories)
            .HasForeignKey(qc => qc.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        string userId = "2db072f6-3706-4996-b222-343896c40606";

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = userId,
            UserName = "admin",
            NormalizedUserName = "ADMIN",
            Email = "admin@admin.com",
            NormalizedEmail = "ADMIN@ADMIN.COM",
            TwoFactorEnabled = false,
            LockoutEnabled = false,
            EmailConfirmed = true,
            PhoneNumberConfirmed = true,
            SecurityStamp = "b4486713-5f7d-134c-96c3-b7c3d441afb4",
            PasswordHash = "AQAAAAIAAYagAAAAEEnc9IcKjqiERt+UMcv/np2qJAJtVMI6qUzqiG5HsoeCyWe0Nr/L2UZC6qmwWTdKjQ==",
            AccessFailedCount = 0,
        });

        string roleIdAdmin = "0344c586-4932-4ee3-8854-65937effcbcf";
        string roleIdUser = "e8486713-5f7d-453b-96c3-b7c3d441afb4";
        
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = roleIdAdmin, Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = roleIdUser, Name = "User", NormalizedName = "USER" }
        );

        modelBuilder.Entity<IdentityUserRole<string>>().HasData(
            new IdentityUserRole<string> { UserId = userId, RoleId = roleIdAdmin },
            new IdentityUserRole<string> { UserId = userId, RoleId = roleIdUser }
        );
        
        modelBuilder.Entity<Question>()
            .HasOne(q => q.CreatedBy)
            .WithMany()
            .HasForeignKey(q => q.CreatedById)
            .OnDelete(DeleteBehavior.Restrict); 
        
        modelBuilder.Entity<Question>()
            .HasOne(q => q.LastUpdatedBy)
            .WithMany()
            .HasForeignKey(q => q.LastUpdatedById)
            .OnDelete(DeleteBehavior.Restrict); 
        
        modelBuilder.Entity<Question>()
            .HasOne(q => q.ApprovedBy)
            .WithMany()
            .HasForeignKey(q => q.ApprovedById)
            .OnDelete(DeleteBehavior.Restrict); 
        
        modelBuilder.Entity<PendingQuestion>()
            .HasOne(q => q.CreatedBy)
            .WithMany()
            .HasForeignKey(q => q.CreatedById)
            .OnDelete(DeleteBehavior.Restrict); 
        
        modelBuilder.Entity<PendingQuestion>()
            .HasOne(q => q.LastUpdatedBy)
            .WithMany()
            .HasForeignKey(q => q.LastUpdatedById)
            .OnDelete(DeleteBehavior.Restrict); 
        
        modelBuilder.Entity<Question>()
            .Property(q => q.ApprovedById)
            .HasDefaultValue(userId); 
        
        modelBuilder.Entity<Question>()
            .Property(q => q.CreatedById)
            .HasDefaultValue(userId);
        
        DateTime utcNow = new DateTime(2025, 1, 1);
        
        modelBuilder.Entity<Question>()
            .Property(q => q.ApprovedAt)
            .HasDefaultValue(utcNow);
        
        modelBuilder.Entity<Question>()
            .Property(q => q.CreatedAt)
            .HasDefaultValue(utcNow);
        
        modelBuilder.Entity<PendingQuestion>()
            .Property(q => q.CreatedById)
            .HasDefaultValue(userId);
        
        modelBuilder.Entity<PendingQuestion>()
            .Property(q => q.CreatedAt)
            .HasDefaultValue(utcNow);
        
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, CategoryName = "General Knowledge" },
            new Category { Id = 2, CategoryName = "Science" },
            new Category { Id = 3, CategoryName = "Physics", ParentCategoryId = 2 },
            new Category { Id = 4, CategoryName = "Chemistry", ParentCategoryId = 2 },
            new Category { Id = 5, CategoryName = "Biology", ParentCategoryId = 2 },
            new Category { Id = 6, CategoryName = "Astronomy", ParentCategoryId = 2 },
            new Category { Id = 7, CategoryName = "Earth Science", ParentCategoryId = 2 },
            new Category { Id = 8, CategoryName = "History" },
            new Category { Id = 9, CategoryName = "Ancient History", ParentCategoryId = 8 },
            new Category { Id = 10, CategoryName = "Medieval History", ParentCategoryId = 8 },
            new Category { Id = 11, CategoryName = "Modern History", ParentCategoryId = 8 },
            new Category { Id = 12, CategoryName = "World War I", ParentCategoryId = 8 },
            new Category { Id = 13, CategoryName = "World War II", ParentCategoryId = 8 },
            new Category { Id = 14, CategoryName = "Geography" },
            new Category { Id = 15, CategoryName = "Countries & Capitals", ParentCategoryId = 14 },
            new Category { Id = 16, CategoryName = "World Landmarks", ParentCategoryId = 14 },
            new Category { Id = 17, CategoryName = "Maps & Flags", ParentCategoryId = 14 },
            new Category { Id = 18, CategoryName = "Mathematics" },
            new Category { Id = 19, CategoryName = "Algebra", ParentCategoryId = 18 },
            new Category { Id = 20, CategoryName = "Geometry", ParentCategoryId = 18 },
            new Category { Id = 21, CategoryName = "Calculus", ParentCategoryId = 18 },
            new Category { Id = 22, CategoryName = "Sports"},
            new Category { Id = 23, CategoryName = "Football", ParentCategoryId = 22 },
            new Category { Id = 24, CategoryName = "Basketball", ParentCategoryId = 22 },
            new Category { Id = 25, CategoryName = "Tennis", ParentCategoryId = 22 },
            new Category { Id = 26, CategoryName = "Olympics", ParentCategoryId = 22 },
            new Category { Id = 27, CategoryName = "Entertainment" },
            new Category { Id = 28, CategoryName = "Movies", ParentCategoryId = 27 },
            new Category { Id = 29, CategoryName = "TV Shows", ParentCategoryId = 27 },
            new Category { Id = 30, CategoryName = "Music", ParentCategoryId = 27 },
            new Category { Id = 31, CategoryName = "Video Games", ParentCategoryId = 27 },
            new Category { Id = 32, CategoryName = "Literature" },
            new Category { Id = 33, CategoryName = "Classic Books", ParentCategoryId = 32 },
            new Category { Id = 34, CategoryName = "Modern Books", ParentCategoryId = 32 },
            new Category { Id = 35, CategoryName = "Mythology" },
            new Category { Id = 36, CategoryName = "Greek Mythology", ParentCategoryId = 35 },
            new Category { Id = 37, CategoryName = "Norse Mythology", ParentCategoryId = 35 },
            new Category { Id = 38, CategoryName = "Egyptian Mythology", ParentCategoryId = 35 },
            new Category { Id = 39, CategoryName = "Technology" },
            new Category { Id = 40, CategoryName = "Computers", ParentCategoryId = 39 },
            new Category { Id = 41, CategoryName = "Internet", ParentCategoryId = 39},
            new Category { Id = 42, CategoryName = "Artificial Intelligence", ParentCategoryId = 39 },
            new Category { Id = 43, CategoryName = "Food & Drinks" },
            new Category { Id = 44, CategoryName = "Cuisine", ParentCategoryId = 43 },
            new Category { Id = 45, CategoryName = "Beverages", ParentCategoryId = 43 },
            new Category { Id = 46, CategoryName = "Religion" },
            new Category { Id = 47, CategoryName = "Christianity", ParentCategoryId = 46 },
            new Category { Id = 48, CategoryName = "Islam", ParentCategoryId = 46 },
            new Category { Id = 49, CategoryName = "Hinduism", ParentCategoryId = 46 },
            new Category { Id = 50, CategoryName = "Buddhism", ParentCategoryId = 46 },
            new Category { Id = 51, CategoryName = "Rock", ParentCategoryId = 30},
            new Category { Id = 52, CategoryName = "Pop", ParentCategoryId = 30 },
            new Category { Id = 53, CategoryName = "Classical", ParentCategoryId = 30 },
            new Category { Id = 54, CategoryName = "Jazz", ParentCategoryId = 30 },
            new Category { Id = 55, CategoryName = "Hip Hop", ParentCategoryId = 30 },
            new Category { Id = 56, CategoryName = "Country", ParentCategoryId = 30 },
            new Category { Id = 57, CategoryName = "Metal", ParentCategoryId = 30 },
            new Category { Id = 58, CategoryName = "Blues", ParentCategoryId = 30 },
            new Category { Id = 59, CategoryName = "Reggae", ParentCategoryId = 30 },
            new Category { Id = 60, CategoryName = "Electronic", ParentCategoryId = 30 },
            new Category { Id = 61, CategoryName = "Folk", ParentCategoryId = 30 },
            new Category { Id = 62, CategoryName = "Opera", ParentCategoryId = 30 },
            new Category { Id = 63, CategoryName = "Movie Soundtracks", ParentCategoryId = 30 },
            new Category { Id = 64, CategoryName = "Music Theory", ParentCategoryId = 30 },
            new Category { Id = 65, CategoryName = "Comedy", ParentCategoryId = 28 },
            new Category { Id = 66, CategoryName = "Horror", ParentCategoryId = 28 },
            new Category { Id = 67, CategoryName = "Action", ParentCategoryId = 28 },
            new Category { Id = 68, CategoryName = "Romance", ParentCategoryId = 28 },
            new Category { Id = 69, CategoryName = "Animation", ParentCategoryId = 28 },
            new Category { Id = 70, CategoryName = "Fantasy", ParentCategoryId = 28 },
            new Category { Id = 71, CategoryName = "Adventure", ParentCategoryId = 28 },
            new Category { Id = 72, CategoryName = "Drama", ParentCategoryId = 28 },
            new Category { Id = 73, CategoryName = "Thriller", ParentCategoryId = 28 },
            new Category { Id = 74, CategoryName = "Comedy", ParentCategoryId = 28 },
            new Category { Id = 75, CategoryName = "Comedy", ParentCategoryId = 29 },
            new Category { Id = 76, CategoryName = "Documentary", ParentCategoryId = 29 },
            new Category { Id = 77, CategoryName = "Drama", ParentCategoryId = 29 },
            new Category { Id = 78, CategoryName = "Reality TV", ParentCategoryId = 29 },
            new Category { Id = 79, CategoryName = "Animation", ParentCategoryId = 29 },
            new Category { Id = 80, CategoryName = "Sitcom", ParentCategoryId = 29 },
            new Category { Id = 81, CategoryName = "Game show", ParentCategoryId = 29 },
            new Category { Id = 82, CategoryName = "Sci-fi", ParentCategoryId = 29 }
        );
    }
}
