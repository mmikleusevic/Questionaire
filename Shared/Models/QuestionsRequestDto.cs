using Newtonsoft.Json;

namespace Shared.Models;

[JsonObject]
public class QuestionsRequestDto
{
    [JsonProperty] public int PageNumber { get; set; }

    [JsonProperty] public int PageSize { get; set; }

    [JsonProperty] public bool OnlyMyQuestions { get; set; }

    [JsonProperty] public string SearchQuery { get; set; } = string.Empty;

    [JsonProperty] public bool FetchApprovedQuestions { get; set; } = true;
}