using System.Collections;

namespace Web.Models;

public class Category
{
    public int Id { get; set; }
    public string CategoryName { get; set; }
    public int? ParentCategoryId { get; set; }
    public List<Category>? ChildCategories { get; set; }
}