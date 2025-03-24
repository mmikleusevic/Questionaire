using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using SharedStandard.Models;

namespace Shared.Models;

[JsonObject]
public class AnswerExtendedDto : AnswerDto
{
    public AnswerExtendedDto()
    {
    }

    public AnswerExtendedDto(int id) : base(id)
    {
    }

    [JsonProperty]
    [Required(ErrorMessage = "Answer Text is required")]
    [StringLength(500, ErrorMessage = "Answer Text must be between 1 and 100 characters", MinimumLength = 1)]
    public override string AnswerText { get; set; }
}