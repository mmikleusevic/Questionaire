using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Models;

namespace QuestionaireApi;

public class QuestionaireDbContext(DbContextOptions<QuestionaireDbContext> options) : DbContext(options)
{
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();

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
    }
}