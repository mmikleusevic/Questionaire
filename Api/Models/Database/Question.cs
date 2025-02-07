using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuestionaireApi.Models;

[Index(nameof(Id))]
public class Question
{
    public int Id { get; set; }
    
    [Required]
    public string QuestionText { get; set; } = null!;
    
    public virtual ICollection<QuestionCategory> QuestionCategories { get; set; } = new List<QuestionCategory>();
    
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
}