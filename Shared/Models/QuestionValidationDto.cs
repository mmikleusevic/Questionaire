using System.ComponentModel.DataAnnotations;
using SharedStandard.Models;

namespace Shared.Models;

public class QuestionValidationDto : QuestionDto
{
    public QuestionValidationDto()
    {
    }

    public QuestionValidationDto(int id) : base(id)
    {
    }

    [Required(ErrorMessage = "Question Text is required")]
    [StringLength(500, ErrorMessage = "Question Text must be between 1 and 500 characters", MinimumLength = 1)]
    public new string QuestionText { get; set; }

    [ValidateComplexType] public new List<AnswerValidationDto> Answers { get; set; } = new List<AnswerValidationDto>();

    [ValidateComplexType] public new List<CategoryValidationDto> Categories { get; set; } = new List<CategoryValidationDto>();
}