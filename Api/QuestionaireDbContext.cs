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
                CategoryName = "Geografija"
            },
            new Category
            {
                Id = 2,
                CategoryName = "Povijest"
            },
            new Category
            {
                Id = 3,
                CategoryName = "Znanost"
            },
            new Category
            {
                Id = 4,
                CategoryName = "Sport"
            },
            new Category
            {
                Id = 5,
                CategoryName = "Glazba"
            },
            new Category
            {
                Id = 6,
                CategoryName = "Kinematografija"
            },
            new Category
            {
                Id = 7,
                CategoryName = "Književnost"
            },
            new Category
            {
                Id = 8,
                CategoryName = "Politika"
            },
            new Category
            {
                Id = 9,
                CategoryName = "Opće znanje"
            }
        );
    }
}