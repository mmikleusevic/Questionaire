using Newtonsoft.Json;

namespace Models
{
    [JsonObject]
    public class Answer
    {
        [JsonProperty] 
        public int Id;
        [JsonProperty] 
        public string AnswerText;
        [JsonProperty] 
        public bool isCorrect;
    }
}
