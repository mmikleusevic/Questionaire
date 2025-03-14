using Newtonsoft.Json;

namespace SharedStandard.Models
{
    [JsonObject]
    public class UniqueQuestionRequestDto
    {
        [JsonProperty] public string UserId { get; set; }

        [JsonProperty] public int NumberOfQuestions { get; set; }

        [JsonProperty] public int[] CategoryIds { get; set; }

        [JsonProperty] public bool IsSingleAnswerMode { get; set; }
    }
}