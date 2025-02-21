namespace QuestionaireApi.Models.Dto;

public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageSize { get; set; }

    public int TotalPages { get; set; }
}