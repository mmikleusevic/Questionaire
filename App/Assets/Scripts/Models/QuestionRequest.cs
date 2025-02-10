namespace Models
{
    public class QuestionRequest
    {
        public string UserId { get; set; }
        public int NumberOfQuestions { get; set; }
        public int[] CategoryIds { get; set; }
        public bool IsSingleAnswerMode { get; set; }
    }
}