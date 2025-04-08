using Shared.Models;

namespace QuestionaireApi.Interfaces;

public interface IRoleService
{
    Task<IList<RoleDto>> GetRoles();
}