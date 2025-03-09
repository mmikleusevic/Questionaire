using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Web.Models;

[JsonObject]
public class PendingQuestion
{
    [JsonProperty] public int Id { get; private set; }

    [JsonProperty]
    [Required(ErrorMessage = "Question Text is required")]
    [StringLength(500, ErrorMessage = "Question Text must be between 1 and 500 characters", MinimumLength = 1)]
    public string QuestionText { get; set; }

    [JsonProperty]
    [ValidateComplexType]
    public List<PendingAnswer> PendingAnswers { get; set; } = new List<PendingAnswer>();

    [JsonProperty] [ValidateComplexType] public List<Category> Categories { get; set; } = new List<Category>();
}