using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Models
{
    [JsonObject]
    public class Category
    {
        [JsonProperty] 
        public int Id { get; set; }
        [JsonProperty] 
        public string CategoryName { get; set; }
        [JsonProperty] 
        public int? ParentCategoryId { get; set; }
        [JsonProperty] 
        public List<Category> ChildCategories { get; set; }
        public bool isSelected { get; set; } = true;
    }
}