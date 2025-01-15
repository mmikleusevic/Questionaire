using System.ComponentModel.DataAnnotations;

namespace QuestionaireApi.Models;

public class Category
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = null!;
    
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
}