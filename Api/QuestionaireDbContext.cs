using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Models;

namespace QuestionaireApi;

public class QuestionaireDbContext(DbContextOptions<QuestionaireDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<UserQuestionHistory> UserQuestionHistory => Set<UserQuestionHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Question>()
            .HasOne(q => q.Category)
            .WithMany(c => c.Questions)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Answer>()
            .HasOne(a => a.Question)
            .WithMany(q => q.Answers)
            .OnDelete(DeleteBehavior.Cascade);
        
        modelBuilder.Entity<UserQuestionHistory>()
            .HasOne(h => h.Question)
            .WithMany()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Category>().HasData(
            new Category
            {
                Id = 1,
                CategoryName = "Geography"
            },
            new Category
            {
                Id = 2,
                CategoryName = "History"
            },
            new Category
            {
                Id = 3,
                CategoryName = "Science"
            },
            new Category
            {
                Id = 4,
                CategoryName = "Sports"
            },
            new Category
            {
                Id = 5,
                CategoryName = "Music"
            },
            new Category
            {
                Id = 6,
                CategoryName = "Cinematography"
            },
            new Category
            {
                Id = 7,
                CategoryName = "Literature"
            },
            new Category
            {
                Id = 8,
                CategoryName = "Politics"
            },
            new Category
            {
                Id = 9,
                CategoryName = "General Knowledge"
            }
        );
    }
}