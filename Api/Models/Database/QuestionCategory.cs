using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuestionaireApi.Models.Database;

[Index(nameof(QuestionId), nameof(CategoryId))]
public class QuestionCategory
{
    public int QuestionId { get; set; }
    public int CategoryId { get; set; }

    [ForeignKey(nameof(QuestionId))] public virtual Question Question { get; set; } = null!;

    [ForeignKey(nameof(CategoryId))] public virtual Category Category { get; set; } = null!;
}