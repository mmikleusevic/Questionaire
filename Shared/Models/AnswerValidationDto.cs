using System.ComponentModel.DataAnnotations;
using SharedStandard.Models;

namespace Shared.Models;

public class AnswerValidationDto : AnswerDto
{
    public AnswerValidationDto()
    {
    }

    public AnswerValidationDto(int id) : base(id)
    {
    }

    [Required(ErrorMessage = "Answer Text is required")]
    [StringLength(500, ErrorMessage = "Answer Text must be between 1 and 100 characters", MinimumLength = 1)]
    public new string AnswerText { get; set; }
}