using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuestionaireApi.Models;

[Index(nameof(QuestionId))]
public class Answer
{
    public int Id { get; set; }
    
    [Required]
    public string AnswerText { get; set; } = null!;
    
    public bool IsCorrect { get; set; }
    
    public int QuestionId { get; set; }
    
    [ForeignKey(nameof(QuestionId))]
    public virtual Question Question { get; set; } = null!;
}