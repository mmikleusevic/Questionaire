using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;

namespace QuestionaireApi.Services;

public class RoleService(RoleManager<IdentityRole> roleManager) : IRoleService
{
    public async Task<IList<string>> GetRoles()
    {
        try
        {
            List<string?> roles = await roleManager.Roles.Select(r => r.Name).ToListAsync();
            return roles;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving roles.", ex);
        }
    }
}