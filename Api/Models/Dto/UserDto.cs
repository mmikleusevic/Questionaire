using System.ComponentModel.DataAnnotations;

namespace QuestionaireApi.Models.Dto;

public class UserDto
{
    public int Id { get; set; }
    [Required]
    public string Name { get; set; } = null!;
    
    public int RoleId { get; set; }
}