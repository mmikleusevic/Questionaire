using System;
using System.Collections.Generic;

namespace Models
{
    [Serializable]
    public class Question
    {
        public int Id;
        public string QuestionText;
        public int CategoryId;
        public List<Answer> Answers;
        public bool isRead = false;
    }
}
