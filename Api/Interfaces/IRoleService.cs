using QuestionaireApi.Models.Database;

namespace QuestionaireApi.Interfaces;

public interface IRoleService
{
    Task<List<Role>> GetRolesAsync();
    Task<Role?> GetRoleByIdAsync(int id);
    Task CreateRoleAsync(Role role);
    Task<bool> UpdateRoleAsync(int id, Role updatedRole);
    Task<bool> DeleteRoleAsync(int id);
}