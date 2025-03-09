using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuestionaireApi.Models.Database;

[Index(nameof(UserId), nameof(QuestionId))]
public class UserQuestionHistory
{
    public int Id { get; set; }

    public string UserId { get; set; }

    public int QuestionId { get; set; }

    [ForeignKey(nameof(QuestionId))] public virtual Question Question { get; set; } = null!;

    public int RoundNumber { get; set; }
}