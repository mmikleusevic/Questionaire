using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace QuestionaireApi.Models.Database;

[Index(nameof(ParentCategoryId))]
[Index(nameof(CategoryName), IsUnique = true)]
public class Category
{
    public int Id { get; set; }

    [Required] [StringLength(100)] public string CategoryName { get; set; } = null!;

    public int? ParentCategoryId { get; set; }

    [ForeignKey(nameof(ParentCategoryId))] public virtual Category? ParentCategory { get; set; }

    public virtual ICollection<Category> ChildCategories { get; set; } = new List<Category>();

    public virtual ICollection<QuestionCategory> QuestionCategories { get; set; } = new List<QuestionCategory>();
}