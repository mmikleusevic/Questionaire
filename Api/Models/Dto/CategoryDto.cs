namespace QuestionaireApi.Models.Dto;

public class CategoryDto
{
    public int Id { get; set; }
    public string CategoryName { get; set; }
    public int? ParentCategoryId { get; set; }
    public List<CategoryDto> ChildCategories { get; set; } = new List<CategoryDto>();
}