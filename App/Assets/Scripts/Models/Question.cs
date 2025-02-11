using System.Collections.Generic;
using Newtonsoft.Json;

namespace Models
{
    [JsonObject]
    public class Question
    {
        [JsonProperty] 
        public int Id;
        [JsonProperty] 
        public string QuestionText;
        [JsonProperty] 
        public int CategoryId;
        [JsonProperty]
        public List<Answer> Answers;
        public bool isRead = false;
    }
}
