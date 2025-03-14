using System.Collections.Generic;
using Newtonsoft.Json;

namespace SharedStandard.Models
{
    [JsonObject]
    public class QuestionDto
    {
        [JsonProperty] public List<AnswerDto> Answers;

        [JsonProperty] public int CategoryId;

        [JsonProperty] public int Id;

        public bool isRead;

        [JsonProperty] public string QuestionText;
    }
}