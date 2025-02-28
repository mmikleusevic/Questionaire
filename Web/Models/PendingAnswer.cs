using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Web.Models;

[JsonObject]
public class PendingAnswer
{
    [JsonProperty]
    public int Id { get; private set; }
    [JsonProperty]
    [Required(ErrorMessage = "Answer Text is required")]
    [StringLength(500, ErrorMessage = "Answer Text must be between 1 and 100 characters", MinimumLength = 1)]
    public string AnswerText { get; set; }
    [JsonProperty]
    public bool IsCorrect { get; set; }
}