namespace QuestionaireApi.Models.Dto;

public class AnswerDto
{
    public int Id { get; set; }
    public string AnswerText { get; set; }
    public bool IsCorrect { get; set; }
}