namespace Web.Interfaces;

public interface IRoleService
{
    Task<IList<string>> GetRoles();
}