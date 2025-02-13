using QuestionaireApi.Models.Database;

namespace QuestionaireApi.Models.Dto;

public class UpdatePendingQuestionRequestDto
{
    public string QuestionText { get; set; }
    public List<PendingAnswer> Answers { get; set; }
    public List<int> CategoryIds { get; set; }
}