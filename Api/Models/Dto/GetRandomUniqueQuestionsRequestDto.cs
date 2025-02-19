namespace QuestionaireApi.Models;

public class GetRandomUniqueQuestionsRequestDto
{
    public string UserId { get; set; }
    public int NumberOfQuestions { get; set; }
    public List<int> CategoryIds { get; set; }
    public bool IsSingleAnswerMode { get; set; }
}