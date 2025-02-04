using System;

namespace Models
{
    [Serializable]
    public class Category
    {
        public int Id { get; set; }
        
        public string CategoryName { get; set; } = null!;

        public bool isUsed { get; set; } = true;
    }
}