namespace QuestionaireApi.Models.Dto;

public class QuestionsRequestDto
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool OnlyMyQuestions { get; set; }
}