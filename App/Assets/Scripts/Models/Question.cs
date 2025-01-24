using System;
using System.Collections.Generic;

namespace DefaultNamespace.Models
{
    [Serializable]
    public class Question
    {
        public int Id;
        public string QuestionText;
        public int CategoryId;
        public List<Answer> Answers;
    }
}