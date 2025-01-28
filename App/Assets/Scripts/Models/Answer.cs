using System;

namespace DefaultNamespace.Models
{
    [Serializable]
    public class Answer
    {
        public int Id;
        public string AnswerText;
        public bool isCorrect;
    }
}