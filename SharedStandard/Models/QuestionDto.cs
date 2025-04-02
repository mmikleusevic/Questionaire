using System.Collections.Generic;
using Newtonsoft.Json;

namespace SharedStandard.Models
{
    [JsonObject]
    public class QuestionDto
    {
        public bool isRead;

        public QuestionDto()
        {
        }

        public QuestionDto(int id)
        {
            Id = id;
        }

        [JsonProperty] public int Id { get; private set; }
        [JsonProperty] public Difficulty Difficulty { get; set; }
        [JsonProperty] public virtual string QuestionText { get; set; }
        [JsonProperty] public List<AnswerDto> Answers { get; set; } = new List<AnswerDto>();
        [JsonProperty] public List<CategoryDto> Categories { get; set; } = new List<CategoryDto>();
    }
}