using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuestionaireApi.Models;

[Index(nameof(CategoryId))]
public class Question
{
    public int Id { get; set; }
    
    [Required]
    public string QuestionText { get; set; }
    
    public int CategoryId { get; set; }
    
    [ForeignKey(nameof(CategoryId))]
    public virtual Category Category { get; set; } = null!;
    public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
}