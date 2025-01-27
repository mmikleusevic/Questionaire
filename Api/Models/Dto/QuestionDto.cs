namespace QuestionaireApi.Models.Dto;

public class QuestionDto
{
    public int Id { get; set; }
    public string QuestionText { get; set; }
    public List<AnswerDto> Answers { get; set; }
}