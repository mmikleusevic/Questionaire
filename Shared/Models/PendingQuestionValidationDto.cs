using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Shared.Models;

[JsonObject]
public class PendingQuestionValidationDto
{
    [JsonProperty] public int Id { get; set; }

    [JsonProperty] public string QuestionText { get; set; }

    [JsonProperty]
    [ValidateComplexType]
    public List<PendingAnswerValidationDto> PendingAnswers { get; set; } = new List<PendingAnswerValidationDto>();

    [JsonProperty]
    [ValidateComplexType]
    public List<CategoryValidationDto> Categories { get; set; } = new List<CategoryValidationDto>();
}