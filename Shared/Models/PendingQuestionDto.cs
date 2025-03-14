using Newtonsoft.Json;
using SharedStandard.Models;

namespace Shared.Models;

[JsonObject]
public class PendingQuestionDto
{
    [JsonProperty] public int Id { get; set; }

    [JsonProperty] public string QuestionText { get; set; }

    [JsonProperty] public List<PendingAnswerDto> PendingAnswers { get; set; } = new List<PendingAnswerDto>();

    [JsonProperty] public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
}