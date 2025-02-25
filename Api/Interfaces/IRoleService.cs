using QuestionaireApi.Models.Database;

namespace QuestionaireApi.Interfaces;

public interface IRoleService
{
    Task<List<Role>> GetRoles();
    Task<Role?> GetRoleById(int id);
    Task CreateRole(Role role);
    Task<bool> UpdateRole(int id, Role updatedRole);
    Task<bool> DeleteRole(int id);
}