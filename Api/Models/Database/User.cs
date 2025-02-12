using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestionaireApi.Models.Database;

public class User
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
    public int RoleId { get; set; }
    
    [ForeignKey(nameof(RoleId))]
    public virtual Role Role { get; set; } = null!;
}