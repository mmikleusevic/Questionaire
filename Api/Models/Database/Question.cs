using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SharedStandard.Models;

namespace QuestionaireApi.Models.Database;

[Index(nameof(QuestionText))]
[Index(nameof(CreatedById))]
[Index(nameof(Difficulty))]
public class Question
{
    public int Id { get; set; }

    [Required] public string QuestionText { get; set; } = null!;

    [Required] public string CreatedById { get; set; } = null!;

    [Required] public Difficulty Difficulty { get; set; } = Difficulty.Medium;

    public string? LastUpdatedById { get; set; }

    public string? ApprovedById { get; set; }

    public string? DeletedById { get; set; }

    [Required] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }

    public bool IsApproved { get; set; }


    [ForeignKey(nameof(CreatedById))] public virtual User CreatedBy { get; set; } = null!;

    [ForeignKey(nameof(LastUpdatedById))] public virtual User? LastUpdatedBy { get; set; }

    [ForeignKey(nameof(ApprovedById))] public virtual User? ApprovedBy { get; set; }

    public virtual ICollection<QuestionCategory> QuestionCategories { get; set; } = new List<QuestionCategory>();

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
}