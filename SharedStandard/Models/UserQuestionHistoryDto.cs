using System.Collections.Generic;
using Newtonsoft.Json;

namespace SharedStandard.Models
{
    [JsonObject]
    public class UserQuestionHistoryDto
    {
        [JsonProperty] public string UserId { get; set; } = null!;
        [JsonProperty] public List<int> QuestionIds { get; set; } = new List<int>();
    }
}