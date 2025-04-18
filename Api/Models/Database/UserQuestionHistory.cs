using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionaireApi.Models.Database;

public class UserQuestionHistory
{
    public int Id { get; set; }

    public string UserId { get; set; }

    public int QuestionId { get; set; }

    [ForeignKey(nameof(QuestionId))] public virtual Question Question { get; set; } = null!;
}