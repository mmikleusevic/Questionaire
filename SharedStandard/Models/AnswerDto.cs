using Newtonsoft.Json;

namespace SharedStandard.Models
{
    [JsonObject]
    public class AnswerDto
    {
        public AnswerDto()
        {
        }

        public AnswerDto(int id)
        {
            Id = id;
        }

        [JsonProperty] public int Id { get; private set; }
        [JsonProperty] public string AnswerText { get; set; }
        [JsonProperty] public bool IsCorrect { get; set; }
    }
}