using System.ComponentModel.DataAnnotations;

namespace QuestionaireApi.Models.Database;

public class Role
{
    public int Id { get; set; }

    [Required] public string Name { get; set; } = null!;
    
    public virtual ICollection<User> Users { get; set; } = null!;
}