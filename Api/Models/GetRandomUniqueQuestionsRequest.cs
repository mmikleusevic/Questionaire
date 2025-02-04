namespace QuestionaireApi.Models;

public class GetRandomUniqueQuestionsRequest
{
    public string UserId { get; set; }
    public int NumberOfQuestions { get; set; }
    public int[] CategoryIds { get; set; }
}