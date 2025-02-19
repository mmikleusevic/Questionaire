using System.ComponentModel.DataAnnotations;

namespace QuestionaireApi.Models.Dto;

public class RoleDto
{
    public int Id { get; set; }

    [Required] 
    public string Name { get; set; } = null!;
}