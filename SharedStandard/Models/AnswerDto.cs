using Newtonsoft.Json;

namespace SharedStandard.Models
{
    [JsonObject]
    public class AnswerDto
    {
        [JsonProperty] public string AnswerText;

        [JsonProperty] public int Id;

        [JsonProperty] public bool isCorrect;
    }
}