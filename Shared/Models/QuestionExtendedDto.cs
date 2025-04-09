using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using SharedStandard.Models;

namespace Shared.Models;

[JsonObject]
public class QuestionExtendedDto : QuestionDto
{
    public QuestionExtendedDto()
    {
    }

    public QuestionExtendedDto(int id) : base(id)
    {
    }

    [JsonProperty]
    [Required(ErrorMessage = "Question Text is required")]
    [StringLength(500, ErrorMessage = "Question Text must be between 1 and 500 characters", MinimumLength = 1)]
    public override string QuestionText { get; set; }
    
    [JsonProperty]
    public string? CreatedById { get; set; }

    [ValidateComplexType]
    [JsonProperty]
    public new List<AnswerExtendedDto> Answers { get; set; } = new List<AnswerExtendedDto>();

    [ValidateComplexType]
    [JsonProperty]
    public new List<CategoryExtendedDto> Categories { get; set; } = new List<CategoryExtendedDto>();
}