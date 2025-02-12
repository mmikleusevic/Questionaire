using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace QuestionaireApi.Models.Database;

[Index(nameof(Id))]
public class PendingQuestion
{
    public int Id { get; set; }

    [Required]
    public string QuestionText { get; set; } = null!;

    [Required]
    public virtual ICollection<PendingAnswer> Answers { get; set; } = new List<PendingAnswer>();

    [Required]
    public virtual ICollection<PendingQuestionCategory> PendingQuestionCategories { get; set; } = new List<PendingQuestionCategory>();

    public bool IsApproved { get; set; } = false;
}