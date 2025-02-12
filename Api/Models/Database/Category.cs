using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Models.Database;

namespace QuestionaireApi.Models;

[Index(nameof(ParentCategoryId))]
public class Category
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string CategoryName { get; set; } = null!;
    
    public int? ParentCategoryId { get; set; }
    
    [ForeignKey(nameof(ParentCategoryId))]
    public virtual Category? ParentCategory { get; set; }
    
    public virtual ICollection<Category> ChildCategories { get; set; } = new List<Category>();
    
    public virtual ICollection<QuestionCategory> QuestionCategories { get; set; } = new List<QuestionCategory>();
    
    public virtual ICollection<PendingQuestionCategory> PendingQuestionCategories { get; set; } = new List<PendingQuestionCategory>();
}