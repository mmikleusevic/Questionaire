using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuestionaireApi.Models.Database;

[Index(nameof(Id), nameof(CreatedById), nameof(ApprovedById))]
[Index(nameof(QuestionText))]
public class Question
{
    public int Id { get; set; }

    [Required] public string QuestionText { get; set; } = null!;

    [Required] public string CreatedById { get; set; } = null!;

    public string? LastUpdatedById { get; set; }

    [Required] public string ApprovedById { get; set; } = null!;

    [Required] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastUpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    [Required] public DateTime ApprovedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(CreatedById))] public virtual User CreatedBy { get; set; } = null!;

    [ForeignKey(nameof(LastUpdatedById))] public virtual User? LastUpdatedBy { get; set; }

    [ForeignKey(nameof(ApprovedById))] public virtual User ApprovedBy { get; set; } = null!;

    public virtual ICollection<QuestionCategory> QuestionCategories { get; set; } = new List<QuestionCategory>();

    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
}