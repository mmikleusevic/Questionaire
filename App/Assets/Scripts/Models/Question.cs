using System.Collections.Generic;
using Newtonsoft.Json;
using SharedStandard.Models;

namespace Models
{
    [JsonObject]
    public class Question
    {
        [JsonProperty] public List<AnswerDto> Answers;

        [JsonProperty] public int CategoryId;

        [JsonProperty] public int Id;

        public bool isRead;

        [JsonProperty] public string QuestionText;
    }
}