using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuestionaireApi.Models.Database;

[Index(nameof(PendingQuestionId))]
public class PendingAnswer
{
    public int Id { get; set; }

    [Required] 
    public string AnswerText { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public int PendingQuestionId { get; set; }

    [ForeignKey(nameof(PendingQuestionId))]
    public virtual PendingQuestion PendingQuestion { get; set; } = null!;
}