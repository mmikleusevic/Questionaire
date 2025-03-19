using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Shared.Models;

[JsonObject]
public class PendingQuestionDto
{
    [JsonProperty] public int Id { get; set; }

    [JsonProperty] public string QuestionText { get; set; }

    [JsonProperty]
    [ValidateComplexType]
    public List<PendingAnswerDto> PendingAnswers { get; set; } = new List<PendingAnswerDto>();

    [JsonProperty]
    [ValidateComplexType]
    public List<CategoryExtendedDto> Categories { get; set; } = new List<CategoryExtendedDto>();
}