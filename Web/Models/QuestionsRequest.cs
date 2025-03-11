namespace Web.Models;

public class QuestionsRequest
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public bool OnlyMyQuestions { get; set; }
}