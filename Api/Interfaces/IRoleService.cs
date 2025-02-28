using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Interfaces;

public interface IRoleService
{
    Task<List<Role>> GetRoles();
    Task<Role?> GetRoleById(int id);
    Task CreateRole(RoleDto role);
    Task<bool> UpdateRole(int id, RoleDto updatedRole);
    Task<bool> DeleteRole(int id);
}