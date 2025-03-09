namespace QuestionaireApi.Models.Dto;

public class PendingQuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; }
    public List<PendingAnswerDto> PendingAnswers { get; set; }
    public List<CategoryDto> Categories { get; set; }
}