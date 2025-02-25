namespace QuestionaireApi.Models.Dto;

public class CategoriesDto
{
    public List<CategoryDto> NestedCategories { get; set; }
    public List<CategoryDto> FlatCategories { get; set; }
}