using Shared.Models;

namespace Web.Interfaces;

public interface IRoleService
{
    Task<IList<RoleDto>> GetRoles();
}