using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Shared.Models;

[JsonObject]
public class PendingAnswerDto
{
    public PendingAnswerDto()
    {
    }

    public PendingAnswerDto(int id)
    {
        Id = id;
    }

    [JsonProperty] public int Id { get; private set; }

    [JsonProperty]
    [Required(ErrorMessage = "Answer Text is required")]
    [StringLength(500, ErrorMessage = "Answer Text must be between 1 and 100 characters", MinimumLength = 1)]
    public string AnswerText { get; set; }

    [JsonProperty] public bool IsCorrect { get; set; }
}