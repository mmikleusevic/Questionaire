namespace QuestionaireApi.Models.Dto;

public class PendingAnswerDto
{
    public int Id { get; set; }
    public string AnswerText { get; set; }
    public bool IsCorrect { get; set; }
}