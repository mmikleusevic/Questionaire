using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace QuestionaireApi.Models.Database;

[Index(nameof(Id), nameof(CreatedById))]
public class PendingQuestion
{
    public int Id { get; set; }

    [Required]
    public string QuestionText { get; set; } = null!;
    
    [Required]
    public string CreatedById { get; set; } = null!;
    
    public string? LastUpdatedById { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastUpdatedAt { get; set; }
    
    [ForeignKey(nameof(CreatedById))]
    public virtual User CreatedBy { get; set; } = null!;
    
    [ForeignKey(nameof(LastUpdatedById))]
    public virtual User? LastUpdatedBy { get; set; }

    [Required]
    public virtual ICollection<PendingAnswer> PendingAnswers { get; set; } = new List<PendingAnswer>();

    [Required]
    public virtual ICollection<PendingQuestionCategory> PendingQuestionCategories { get; set; } = new List<PendingQuestionCategory>();
}