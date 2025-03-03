using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuestionaireApi.Models.Database;

[Index(nameof(PendingQuestionId), nameof(CategoryId))]
public class PendingQuestionCategory
{
    public int PendingQuestionId { get; set; }
    public int CategoryId { get; set; }

    [ForeignKey(nameof(PendingQuestionId))]
    public virtual PendingQuestion PendingQuestion { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))]
    public virtual Category Category { get; set; } = null!;
}