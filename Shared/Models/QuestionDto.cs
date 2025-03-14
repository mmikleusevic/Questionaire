using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using SharedStandard.Models;

namespace Shared.Models;

[JsonObject]
public class QuestionDto
{
    public QuestionDto()
    {
    }

    public QuestionDto(int id)
    {
        Id = id;
    }

    [JsonProperty] public int Id { get; private set; }

    [JsonProperty]
    [Required(ErrorMessage = "Question Text is required")]
    [StringLength(500, ErrorMessage = "Question Text must be between 1 and 500 characters", MinimumLength = 1)]
    public string QuestionText { get; set; }

    [JsonProperty] [ValidateComplexType] public List<AnswerDto> Answers { get; set; } = new List<AnswerDto>();

    [JsonProperty] [ValidateComplexType] public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
}