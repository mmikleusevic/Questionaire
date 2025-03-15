namespace QuestionaireApi.Interfaces;

public interface IRoleService
{
    Task<IList<string>> GetRoles();
}