using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using Shared.Models;

namespace QuestionaireApi.Services;

public class RoleService(RoleManager<IdentityRole> roleManager) : IRoleService
{
    public async Task<IList<RoleDto>> GetRoles()
    {
        try
        {
            List<IdentityRole> roles = await roleManager.Roles.ToListAsync();

            List<RoleDto> rolesDto = new List<RoleDto>();

            foreach (IdentityRole role in roles)
            {
                if (string.IsNullOrEmpty(role.Name)) continue;

                rolesDto.Add(new RoleDto
                {
                    RoleName = role.Name
                });
            }

            return rolesDto;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("An error occurred while retrieving roles.", ex);
        }
    }
}